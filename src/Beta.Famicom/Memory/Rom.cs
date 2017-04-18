namespace Beta.Famicom.Memory
{
    public sealed class Rom : IMemory
    {
        private byte[] buffer;
        private int mask;

        public Rom(byte[] dump)
        {
            buffer = (byte[])dump.Clone();
            mask = dump.Length - 1;
        }

        public void read(int address, ref byte data)
        {
            data = buffer[address & mask];
        }

        public void write(int address, byte data)
        {
            // read only :-)
        }
    }
}
