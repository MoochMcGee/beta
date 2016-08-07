using Beta.Platform.Exceptions;
using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;
using Beta.Famicom.CPU;

namespace Beta.Famicom.Boards.Nintendo
{
    [BoardName("NES-FxROM")]
    public class NintendoMmc4 : Board
    {
        private int[] chrPages;
        private int[] prgPages;

        private int chrTimer;
        private int chr0, chr0Latch;
        private int chr1, chr1Latch;
        private int mirroring;

        public NintendoMmc4(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[4];
            prgPages = new int[2];
            prgPages[0] = +0 << 14;
            prgPages[1] = -1 << 14;

            chr0 = chr0Latch = 0;
            chr1 = chr1Latch = 2;
        }

        private void PokeA000(ushort address, byte data)
        {
            prgPages[0] = data << 14;
        }

        private void PokeB000(ushort address, byte data)
        {
            chrPages[0] = data << 12;
        }

        private void PokeC000(ushort address, byte data)
        {
            chrPages[1] = data << 12;
        }

        private void PokeD000(ushort address, byte data)
        {
            chrPages[2] = data << 12;
        }

        private void PokeE000(ushort address, byte data)
        {
            chrPages[3] = data << 12;
        }

        private void PokeF000(ushort address, byte data)
        {
            mirroring = (data & 0x01);
        }

        protected override int DecodeChr(ushort address)
        {
            switch ((address >> 12) & 1)
            {
            case 0: return chrPages[chr0] | (address & 0xfff);
            case 1: return chrPages[chr1] | (address & 0xfff);
            }

            throw new CompilerPleasingException();
        }

        protected override int DecodePrg(ushort address)
        {
            return prgPages[(address >> 14) & 1] | (address & 0x3fff);
        }

        public override void MapToCpu(R2A03Bus bus)
        {
            base.MapToCpu(bus);

            bus.Map("1010 ---- ---- ----", writer: PokeA000);
            bus.Map("1011 ---- ---- ----", writer: PokeB000);
            bus.Map("1100 ---- ---- ----", writer: PokeC000);
            bus.Map("1101 ---- ---- ----", writer: PokeD000);
            bus.Map("1110 ---- ---- ----", writer: PokeE000);
            bus.Map("1111 ---- ---- ----", writer: PokeF000);
        }

        public override void Consume(PpuAddressSignal e)
        {
            if (chrTimer != 0 && --chrTimer == 0)
            {
                chr0 = chr0Latch;
                chr1 = chr1Latch;
            }

            switch (e.Address & 0x1ff0)
            {
            case 0x0fd0: chr0Latch = 0; chrTimer = 2; break;
            case 0x0fe0: chr0Latch = 1; chrTimer = 2; break;
            case 0x1fd0: chr1Latch = 2; chrTimer = 2; break;
            case 0x1fe0: chr1Latch = 3; chrTimer = 2; break;
            }
        }

        public override bool VRAM(ushort address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: a10 = x; return true;
            case 1: a10 = y; return true;
            }

            throw new CompilerPleasingException();
        }
    }
}
