using Beta.Famicom.Formats;
using Beta.Platform.Exceptions;

namespace Beta.Famicom.Boards.Konami
{
    public sealed class VRC1 : IBoard
    {
        private CartridgeImage image;

        private int nmt_mode;
        private int[] chr_page = new int[2];
        private int[] prg_page = new int[4];

        public void applyImage(CartridgeImage image)
        {
            this.image = image;

            this.prg_page[3] = 0x1e000;
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
            switch (address & 0xf000)
            {
            case 0x8000: prg_page[0] = (data << 13) & 0x1e000; break;

            case 0x9000:
                nmt_mode = data & 1;
                chr_page[0] = (chr_page[0] & 0xf000) | ((data << 15) & 0x10000);
                chr_page[1] = (chr_page[1] & 0xf000) | ((data << 14) & 0x10000);
                break;

            case 0xa000: prg_page[1] = (data << 13) & 0x1e000; break;
            case 0xb000: break;
            case 0xc000: prg_page[2] = (data << 13) & 0x1e000; break;
            case 0xd000: break;
            case 0xe000: chr_page[0] = (chr_page[0] & 0x10000) | ((data << 12) & 0xf000); break;
            case 0xf000: chr_page[1] = (chr_page[1] & 0x10000) | ((data << 12) & 0xf000); break;
            }
        }

        private int MapR2A03Address(int address)
        {
            switch (address & 0xe000)
            {
            case 0x8000: return (address & 0x1fff) | prg_page[0];
            case 0xa000: return (address & 0x1fff) | prg_page[1];
            case 0xc000: return (address & 0x1fff) | prg_page[2];
            case 0xe000: return (address & 0x1fff) | prg_page[3];
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
            switch (address & 0x1000)
            {
            case 0x0000: return (address & 0xfff) | chr_page[0];
            case 0x1000: return (address & 0xfff) | chr_page[1];
            }

            throw new CompilerPleasingException();
        }

        public bool vram(int address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            a10 = nmt_mode == 0
                ? x
                : y
                ;

            return true;
        }
    }
}
