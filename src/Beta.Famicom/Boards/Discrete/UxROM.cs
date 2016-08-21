using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discrete
{
    [BoardName("U(.+)ROM")]
    public sealed class UxROM : IBoard
    {
        private CartridgeImage image;
        private int prg_page;

        public void ApplyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void R2A03Read(ushort address, ref byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                this.image.prg.Read(MapR2A03Address(address), ref data);
            }
        }

        public void R2A03Write(ushort address, byte data)
        {
            if ((address & 0x8000) == 0x8000)
            {
                prg_page = data;
            }
        }

        private int MapR2A03Address(ushort address)
        {
            return (address & 0xc000) == 0x8000
                ? (address & 0x3fff) | (prg_page << 14)
                : (address & 0x3fff) | (~0 << 14)
                ;
        }

        public void R2C02Read(ushort address, ref byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(address, ref data);
            }
        }

        public void R2C02Write(ushort address, byte data)
        {
            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Write(address, data);
            }
        }

        public bool VRAM(ushort address, out int a10)
        {
            var x = (address >> 10) & image.h;
            var y = (address >> 11) & image.v;

            a10 = x | y;

            return true;
        }
    }
}
