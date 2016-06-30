using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Unlicensed
{
    [BoardName("MLT-ACTION52")]
    public class MltAction52 : Board
    {
        private int prgMode;
        private int[] ram4;
        private int mirroring;
        private int chrPage;
        private int prgPage;

        public MltAction52(CartridgeImage image)
            : base(image)
        {
            ram4 = new int[4];
        }

        private void PeekRam4(ushort address, ref byte data)
        {
            data = (byte)ram4[address & 3];
        }

        private void PokeRam4(ushort address, ref byte data)
        {
            ram4[address & 3] = (data & 0x0f);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x1fff) | chrPage;
        }

        protected override int DecodePrg(ushort address)
        {
            switch (prgMode)
            {
            case 0: return (address & 0x7fff) | (prgPage & ~0x7fff);
            case 1: return (address & 0x3fff) | (prgPage & ~0x3fff);
            }

            return base.DecodePrg(address);
        }

        protected override void PeekPrg(ushort address, ref byte data)
        {
            if (Prg == null)
            {
                return;
            }

            base.PeekPrg(address, ref data);
        }

        protected override void PokePrg(ushort address, ref byte data)
        {
            switch ((address >> 11) & 3)
            {
            case 0: SelectPrg(0); break;
            case 1: SelectPrg(1); break;
            case 2: SelectPrg(-1); break;
            case 3: SelectPrg(2); break;
            }

            chrPage = ((address & 0x000f) << 15) | ((data & 0x03) << 13);
            prgPage = ((address & 0x07c0) << 8);
            prgMode = ((address & 0x0020) >> 5);
            mirroring = ((address & 0x2000) >> 13);
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Map("0100 001- ---- ----", reader: PeekRam4, writer: PokeRam4); // $4200-$43ff
            bus.Map("0100 01-- ---- ----", reader: PeekRam4, writer: PokeRam4); // $4400-$47ff
            bus.Map("0100 1--- ---- ----", reader: PeekRam4, writer: PokeRam4); // $4800-$4fff
            bus.Map("0101 ---- ---- ----", reader: PeekRam4, writer: PokeRam4); // $5000-5ffff
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
