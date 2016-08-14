namespace Beta.Famicom.Memory
{
    public sealed class CGRAM
    {
        private readonly byte[] cgram;

        public CGRAM()
        {
            cgram = new byte[32];
        }

        public byte Read(int address)
        {
            return cgram[MapAddress(address)];
        }

        public void Write(int address, byte data)
        {
            cgram[MapAddress(address)] = data;
        }

        private int MapAddress(int address)
        {
            return (address & 3) == 0
                ? address & 0x000c
                : address & 0x001f
                ;
        }
    }
}
