using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-1")]
    public class KonamiVrc1 : Board
    {
        private int[] chrPages;
        private int[] prgPages;
        private int mirroring;

        public KonamiVrc1(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[2];
            prgPages = new int[4];
            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = +0 << 13;
            prgPages[3] = -1 << 13;
        }

        private void Poke8000(ushort address, ref byte data)
        {
            prgPages[0] = (data & 0x0f) << 13;
        }

        private void Poke9000(ushort address, ref byte data)
        {
            mirroring = (data & 1);
            chrPages[0] = (chrPages[0] & ~0x10000) | ((data & 2) << 15);
            chrPages[1] = (chrPages[1] & ~0x10000) | ((data & 4) << 14);
        }

        private void PokeA000(ushort address, ref byte data)
        {
            prgPages[1] = (data & 0x0f) << 13;
        }

        // $b000
        private void PokeC000(ushort address, ref byte data)
        {
            prgPages[2] = (data & 0x0f) << 13;
        }

        // $d000
        private void PokeE000(ushort address, ref byte data)
        {
            chrPages[0] = (chrPages[0] & ~0xf000) | ((data & 0x0f) << 12);
        }

        private void PokeF000(ushort address, ref byte data)
        {
            chrPages[1] = (chrPages[1] & ~0xf000) | ((data & 0x0f) << 12);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0xfff) | chrPages[(address >> 12) & 1];
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x1fff) | prgPages[(address >> 13) & 3];
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Decode("1000 ---- ---- ----").Poke(Poke8000);
            bus.Decode("1001 ---- ---- ----").Poke(Poke9000);
            bus.Decode("1010 ---- ---- ----").Poke(PokeA000);
            // $b000
            bus.Decode("1100 ---- ---- ----").Poke(PokeC000);
            // $d000
            bus.Decode("1110 ---- ---- ----").Poke(PokeE000);
            bus.Decode("1111 ---- ---- ----").Poke(PokeF000);
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
