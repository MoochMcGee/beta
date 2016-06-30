using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;

namespace Beta.Famicom.Boards.Nintendo
{
    [BoardName("NES-PxROM")]
    public class NintendoMmc2 : Board
    {
        private int[] chrPages;
        private int[] prgPages;
        private int chrTimer;
        private int chr0, chr0Latch;
        private int chr1, chr1Latch;
        private int mirroring;

        public NintendoMmc2(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[4];
            prgPages = new int[4];
            prgPages[0] = +0 << 13;
            prgPages[1] = -3 << 13;
            prgPages[2] = -2 << 13;
            prgPages[3] = -1 << 13;

            chr0 = chr0Latch = 0;
            chr1 = chr1Latch = 2;
        }

        private void PokeA000(ushort address, ref byte data)
        {
            prgPages[0] = data << 13;
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
            case 0: return (address & 0xfff) | chrPages[chr0];
            case 1: return (address & 0xfff) | chrPages[chr1];
            }

            return base.DecodeChr(address);
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x1fff) | prgPages[(address >> 13) & 3];
        }

        public override void MapToCpu(IBus bus)
        {
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
                chr0 = chr0Latch | 0;
                chr1 = chr1Latch | 2;
            }

            switch (e.Address & 0x1ff0)
            {
            case 0x0fd0: chr0Latch = 0; chrTimer = 2; break;
            case 0x0fe0: chr0Latch = 1; chrTimer = 2; break;
            case 0x1fd0: chr1Latch = 0; chrTimer = 2; break;
            case 0x1fe0: chr1Latch = 1; chrTimer = 2; break;
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
