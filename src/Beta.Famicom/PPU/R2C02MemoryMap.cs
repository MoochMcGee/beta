using Beta.Famicom.Memory;

namespace Beta.Famicom.PPU
{
    public static class R2C02MemoryMap
    {
        static readonly byte[] vram = new byte[0x800];
        
        public static void read(int address, ref byte data)
        {
            CartridgeConnector.r2c02Read(address, ref data);

            if ((address & 0x3fff) <= 0x1fff)
            {
                return;
            }

            if (CartridgeConnector.vram(address, out int a10))
            {
                data = vram[(a10 << 10) | (address & 0x3ff)];
            }
        }

        public static void write(int address, byte data)
        {
            CartridgeConnector.r2c02Write(address, data);

            if ((address & 0x3fff) <= 0x1fff)
            {
                return;
            }

            if (CartridgeConnector.vram(address, out int a10))
            {
                vram[(a10 << 10) | (address & 0x3ff)] = data;
            }
        }
    }
}
