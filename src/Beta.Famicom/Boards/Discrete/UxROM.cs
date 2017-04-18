using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discrete
{
    public sealed class UxROM : IBoard
    {
        private CartridgeImage image;
        private int prg_page;

        public void applyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void r2a03Read(int address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                this.image.prg.Read(MapR2A03Address(address), ref data);
            }
        }

        public void r2a03Write(int address, byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                prg_page = data;
            }
        }

        private int MapR2A03Address(int address)
        {
            return (address & 0xc000) == 0x8000
                ? (address & 0x3fff) | (prg_page << 14)
                : (address & 0x3fff) | (~0 << 14)
                ;
        }

        public void r2c02Read(int address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(address, ref data);
            }
        }

        public void r2c02Write(int address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Write(address, data);
            }
        }

        public bool vram(int address, out int a10)
        {
            var x = (address >> 10) & image.h;
            var y = (address >> 11) & image.v;

            a10 = x | y;

            return true;
        }
    }
}
