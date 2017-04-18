using Beta.Famicom.PPU;

namespace Beta.Famicom.Memory
{
    public static class CGRAM
    {
        public static byte read(R2C02State e, int address)
        {
            return e.cgram[mapAddress(address)];
        }

        public static void write(R2C02State e, int address, byte data)
        {
            e.cgram[mapAddress(address)] = data;
        }

        private static int mapAddress(int address)
        {
            return (address & 3) == 0
                ? address & 0x000c
                : address & 0x001f
                ;
        }
    }
}
