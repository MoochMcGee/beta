using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discrete
{
    [BoardName("(HVC|NES)-NROM-(128|256)")]
    public sealed class NROM : IBoard
    {
        private CartridgeImage image;

        public void ApplyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void R2A03Read(int address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                image.prg.Read(address, ref data);
            }
        }

        public void R2A03Write(int address, byte data) { }

        public void R2C02Read(int address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(address, ref data);
            }
        }

        public void R2C02Write(int address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(address, ref data);
            }
        }

        public bool VRAM(int address, out int a10)
        {
            var x = (address >> 10) & image.h;
            var y = (address >> 11) & image.v;

            a10 = x | y;

            return true;
        }
    }
}
