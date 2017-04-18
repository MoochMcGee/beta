using Beta.Famicom.Formats;
using Beta.Platform.Exceptions;

namespace Beta.Famicom.Boards.Konami
{
    public sealed class VRC2 : IBoard
    {
        private CartridgeImage image;

        private int nmt_mode;
        private int[] chr_page = new int[8];
        private int[] prg_page = new int[2];

        public void applyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void r2a03Read(int address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                image.prg.Read(MapR2A03Address(address), ref data);
            }
        }

        public void r2a03Write(int address, byte data)
        {
            const int a0_shift = 0;
            const int a1_shift = 1;

            int a0 = (address >> 0) & 1;
            int a1 = (address >> 1) & 1;

            switch ((address & 0xf000) | (a1 << a1_shift) | (a0 << a0_shift))
            {
            case 0x8000:
            case 0x8001:
            case 0x8002:
            case 0x8003:
                prg_page[0] = (data << 13) & 0x1e000;
                break;

            case 0x9000:
            case 0x9001:
            case 0x9002:
            case 0x9003:
                nmt_mode = (data & 3);
                break;

            case 0xa000:
            case 0xa001:
            case 0xa002:
            case 0xa003:
                prg_page[1] = (data << 13) & 0x1e000;
                break;

            case 0xb000: chr_page[0] = (chr_page[0] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xb001: chr_page[0] = (chr_page[0] & 0x0f) | ((data << 4) & 0xf0); break;
            case 0xb002: chr_page[1] = (chr_page[1] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xb003: chr_page[1] = (chr_page[1] & 0x0f) | ((data << 4) & 0xf0); break;

            case 0xc000: chr_page[2] = (chr_page[2] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xc001: chr_page[2] = (chr_page[2] & 0x0f) | ((data << 4) & 0xf0); break;
            case 0xc002: chr_page[3] = (chr_page[3] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xc003: chr_page[3] = (chr_page[3] & 0x0f) | ((data << 4) & 0xf0); break;

            case 0xd000: chr_page[4] = (chr_page[4] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xd001: chr_page[4] = (chr_page[4] & 0x0f) | ((data << 4) & 0xf0); break;
            case 0xd002: chr_page[5] = (chr_page[5] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xd003: chr_page[5] = (chr_page[5] & 0x0f) | ((data << 4) & 0xf0); break;

            case 0xe000: chr_page[6] = (chr_page[6] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xe001: chr_page[6] = (chr_page[6] & 0x0f) | ((data << 4) & 0xf0); break;
            case 0xe002: chr_page[7] = (chr_page[7] & 0xf0) | ((data << 0) & 0x0f); break;
            case 0xe003: chr_page[7] = (chr_page[7] & 0x0f) | ((data << 4) & 0xf0); break;
            }
        }

        private int MapR2A03Address(int address)
        {
            switch (address & 0xe000)
            {
            case 0x8000: return (address & 0x1fff) | prg_page[0];
            case 0xa000: return (address & 0x1fff) | prg_page[1];
            case 0xc000: return (address & 0x1fff) | (~1 << 13);
            case 0xe000: return (address & 0x1fff) | (~0 << 13);
            }

            throw new CompilerPleasingException();
        }

        public void r2c02Read(int address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(MapR2C02Address(address), ref data);
            }
        }

        public void r2c02Write(int address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Write(MapR2C02Address(address), data);
            }
        }

        private int MapR2C02Address(int address)
        {
            const int chr_shift = 10;

            switch (address & 0x1c00)
            {
            case 0x0000: return (address & 0x3ff) | ((chr_page[0] << chr_shift) & 0x3fc00);
            case 0x0400: return (address & 0x3ff) | ((chr_page[1] << chr_shift) & 0x3fc00);
            case 0x0800: return (address & 0x3ff) | ((chr_page[2] << chr_shift) & 0x3fc00);
            case 0x0c00: return (address & 0x3ff) | ((chr_page[3] << chr_shift) & 0x3fc00);
            case 0x1000: return (address & 0x3ff) | ((chr_page[4] << chr_shift) & 0x3fc00);
            case 0x1400: return (address & 0x3ff) | ((chr_page[5] << chr_shift) & 0x3fc00);
            case 0x1800: return (address & 0x3ff) | ((chr_page[6] << chr_shift) & 0x3fc00);
            case 0x1c00: return (address & 0x3ff) | ((chr_page[7] << chr_shift) & 0x3fc00);
            }

            throw new CompilerPleasingException();
        }

        public bool vram(int address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (nmt_mode)
            {
            case 0: a10 = x; return true;
            case 1: a10 = y; return true;
            case 2: a10 = 0; return true;
            case 3: a10 = 1; return true;
            }

            throw new CompilerPleasingException();
        }
    }
}
