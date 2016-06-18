using Beta.Platform.Exceptions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Nintendo
{
    [BoardName("HVC-SGROM")]
    [BoardName("NES-SGROM")]
    [BoardName("NES-SKROM")]
    [BoardName("NES-SLROM")]
    [BoardName("NES-SNROM")]
    [BoardName("NES-SUROM")]
    [BoardName("NES-SxROM")]
    public class NintendoMmc1 : Board
    {
        private int[] chrPages;
        private int[] prgPages;
        private int mirroring;
        private int chrMode;
        private int prgMode = 3;
        private int shift;
        private int value;

        public NintendoMmc1(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[2];
            prgPages = new int[1];
        }

        private void Poke8000()
        {
            mirroring = (value & 0x03);
            prgMode = (value & 0x0c) >> 2;
            chrMode = (value & 0x10) >> 4;
        }

        private void PokeA000()
        {
            chrPages[0] = (value & 0x1f) << 12;
        }

        private void PokeC000()
        {
            chrPages[1] = (value & 0x1f) << 12;
        }

        private void PokeE000()
        {
            prgPages[0] = (value & 0x0f) << 14;
        }

        protected override int DecodeChr(ushort address)
        {
            switch (chrMode)
            {
            case 0: return (address & 0x1fff) | (chrPages[0] & ~0x1fff);
            case 1:
                switch (address & 0x1000)
                {
                case 0x0000: return (address & 0xfff) | chrPages[0];
                case 0x1000: return (address & 0xfff) | chrPages[1];
                }
                break;
            }

            return base.DecodeChr(address);
        }

        protected override int DecodePrg(ushort address)
        {
            switch (prgMode)
            {
            case 0:
            case 1: return (address & 0x7fff) | (prgPages[0] & ~0x7fff);
            case 2:
                switch (address & 0xc000)
                {
                case 0x8000: return (address & 0x3fff) | 0x00 << 14;
                case 0xc000: return (address & 0x3fff) | prgPages[0];
                }
                break;

            case 3:
                switch (address & 0xc000)
                {
                case 0x8000: return (address & 0x3fff) | prgPages[0];
                case 0xc000: return (address & 0x3fff) | 0x0f << 14;
                }
                break;
            }

            return base.DecodePrg(address);
        }

        protected override void PokePrg(ushort address, ref byte data)
        {
            if (Cpu.Edge)
            { // ignore multiple writes
                if (data >= 0x80)
                {
                    prgMode = 3;
                    shift = 0;
                    value = 0;
                }
                else
                {
                    value |= (data & 1) << shift;
                    shift++;

                    if (shift == 5)
                    {
                        switch (address & 0xe000)
                        {
                        case 0x8000: Poke8000(); break;
                        case 0xa000: PokeA000(); break;
                        case 0xc000: PokeC000(); break;
                        case 0xe000: PokeE000(); break;
                        }

                        value = 0;
                        shift = 0;
                    }
                }
            }
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return 0;
            case 1: return 1;
            case 2: return x;
            case 3: return y;
            }

            throw new CompilerPleasingException();
        }
    }
}
