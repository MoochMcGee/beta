namespace Beta.Famicom.Memory
{
    public sealed class Ram : IMemory
    {
        private byte[] buffer;
        private int mask;

        public Ram(int capacity)
        {
            buffer = new byte[capacity];
            mask = capacity - 1;
        }

        public void read(int address, ref byte data)
        {
            data = buffer[address & mask];
        }

        public void write(int address, byte data)
        {
            buffer[address & mask] = data;
        }
    }
}
