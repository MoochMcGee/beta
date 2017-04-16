using Beta.Famicom.PPU;

namespace Beta.Famicom.Memory
{
    public static class CGRAM
    {
        public static byte Read(R2C02State e, int address)
        {
            return e.cgram[MapAddress(address)];
        }

        public static void Write(R2C02State e, int address, byte data)
        {
            e.cgram[MapAddress(address)] = data;
        }

        private static int MapAddress(int address)
        {
            return (address & 3) == 0
                ? address & 0x000c
                : address & 0x001f
                ;
        }
    }
}
