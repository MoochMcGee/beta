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

        public void Read(int address, ref byte data)
        {
            data = buffer[address & mask];
        }

        public void Write(int address, byte data)
        {
            // read only :-)
        }
    }
}
