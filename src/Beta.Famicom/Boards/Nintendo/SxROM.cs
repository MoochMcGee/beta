using Beta.Famicom.Formats;
using Beta.Platform.Exceptions;

namespace Beta.Famicom.Boards.Nintendo
{
    public sealed class SxROM : IBoard
    {
        private CartridgeImage image;

        private int chr_mode;
        private int chr_page_0;
        private int chr_page_1;

        private int prg_mode = 1;
        private int prg_page;
        private int prg_slot = 1;

        private int nmt_mode;

        private int latch;
        private int shift;

        public void applyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void r2a03Read(int address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                image.prg.read(mapR2A03Address(address), ref data);
            }
        }

        public void r2a03Write(int address, byte data)
        {
            if ((address & 0x8000) != 0x8000)
            {
                return;
            }

            if ((data & 0x80) == 0x80)
            {
                latch = 0;
                shift = 0;
                prg_mode = 1;
                prg_slot = 1;
                return;
            }

            latch |= (data & 1) << shift;
            shift++;

            if (shift == 5)
            {
                switch (address & 0xe000)
                {
                case 0x8000:
                    chr_mode = (latch >> 4) & 1;
                    prg_mode = (latch >> 3) & 1;
                    prg_slot = (latch >> 2) & 1;
                    nmt_mode = (latch >> 0) & 3;
                    break;

                case 0xa000: chr_page_0 = (latch << 12) & 0x1f000; break;
                case 0xc000: chr_page_1 = (latch << 12) & 0x1f000; break;

                case 0xe000:
                    prg_page = (latch << 14) & 0x3c000;
                    break;
                }

                latch = 0;
                shift = 0;
            }
        }

        private int mapR2A03Address(int address)
        {
            if (prg_mode == 0)
            {
                return (prg_page & 0x38000) | (address & 0x7fff);
            }
            else
            {
                if (prg_slot == 0)
                {
                    return (address & 0xc000) == 0x8000
                        ? (address & 0x3fff)
                        : (address & 0x3fff) | (prg_page & 0x3c000)
                        ;
                }
                else
                {
                    return (address & 0xc000) == 0x8000
                        ? (address & 0x3fff) | (prg_page & 0x3c000)
                        : (address & 0x3fff) | 0x3c000
                        ;
                }
            }
        }

        public void r2c02Read(int address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.read(mapR2C02Address(address), ref data);
            }
        }

        public void r2c02Write(int address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.write(mapR2C02Address(address), data);
            }
        }

        private int mapR2C02Address(int address)
        {
            if (chr_mode == 0)
            {
                return (chr_page_0 & 0x1e000) | (address & 0x1fff);
            }
            else
            {
                return (address & 0x1000) == 0x0000
                    ? (chr_page_0 & 0x1f000) | (address & 0xfff)
                    : (chr_page_1 & 0x1f000) | (address & 0xfff)
                    ;
            }
        }

        public bool vram(int address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (nmt_mode)
            {
            case 0: a10 = 0; return true;
            case 1: a10 = 1; return true;
            case 2: a10 = x; return true;
            case 3: a10 = y; return true;
            }

            throw new CompilerPleasingException();
        }
    }
}
