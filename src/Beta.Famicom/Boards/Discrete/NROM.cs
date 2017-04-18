using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discrete
{
    public sealed class NROM : IBoard
    {
        private CartridgeImage image;

        public void applyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void r2a03Read(int address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                image.prg.read(address, ref data);
            }
        }

        public void r2a03Write(int address, byte data) { }

        public void r2c02Read(int address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.read(address, ref data);
            }
        }

        public void r2c02Write(int address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.read(address, ref data);
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
