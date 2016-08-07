using Beta.Famicom.Memory;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02MemoryMap
    {
        private readonly CartridgeConnector cartridge;
        private readonly byte[] vram = new byte[2048];

        public R2C02MemoryMap(CartridgeConnector cartridge)
        {
            this.cartridge = cartridge;
        }

        public void Read(ushort address, ref byte data)
        {
            cartridge.R2C02Read(address, ref data);

            if ((address & 0x3fff) <= 0x1fff)
            {
                return;
            }

            int a10;

            if (cartridge.VRAM(address, out a10))
            {
                data = vram[a10 | (address & 0x3ff)];
            }
        }

        public void Write(ushort address, byte data)
        {
            cartridge.R2C02Write(address, data);

            if ((address & 0x3fff) <= 0x1fff)
            {
                return;
            }

            int a10;

            if (cartridge.VRAM(address, out a10))
            {
                vram[a10 | (address & 0x3ff)] = data;
            }
        }
    }
}
