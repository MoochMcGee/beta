using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

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

        private void PokeA000(ushort address, ref byte data)
        {
            prgPages[0] = data << 14;
        }

        private void PokeB000(ushort address, ref byte data)
        {
            chrPages[0] = data << 12;
        }

        private void PokeC000(ushort address, ref byte data)
        {
            chrPages[1] = data << 12;
        }

        private void PokeD000(ushort address, ref byte data)
        {
            chrPages[2] = data << 12;
        }

        private void PokeE000(ushort address, ref byte data)
        {
            chrPages[3] = data << 12;
        }

        private void PokeF000(ushort address, ref byte data)
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

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Decode("1010 ---- ---- ----").Poke(PokeA000);
            bus.Decode("1011 ---- ---- ----").Poke(PokeB000);
            bus.Decode("1100 ---- ---- ----").Poke(PokeC000);
            bus.Decode("1101 ---- ---- ----").Poke(PokeD000);
            bus.Decode("1110 ---- ---- ----").Poke(PokeE000);
            bus.Decode("1111 ---- ---- ----").Poke(PokeF000);
        }

        public override void PpuAddressUpdate(ushort address)
        {
            if (chrTimer != 0 && --chrTimer == 0)
            {
                chr0 = chr0Latch;
                chr1 = chr1Latch;
            }

            switch (address & 0x1ff0)
            {
            case 0x0fd0: chr0Latch = 0; chrTimer = 2; break;
            case 0x0fe0: chr0Latch = 1; chrTimer = 2; break;
            case 0x1fd0: chr1Latch = 2; chrTimer = 2; break;
            case 0x1fe0: chr1Latch = 3; chrTimer = 2; break;
            }
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            }

            throw new CompilerPleasingException();
        }
    }
}
