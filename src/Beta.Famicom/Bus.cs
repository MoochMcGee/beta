using Beta.Famicom.Abstractions;
using Beta.Platform;

namespace Beta.Famicom
{
    public delegate void Reader(ushort address, ref byte data);

    public delegate void Writer(ushort address, ref byte data);

    public class Bus : IBus
    {
        internal readonly Reader[] readers;
        internal readonly Writer[] writers;

        public Bus(int capacity)
        {
            readers = new Reader[capacity];
            writers = new Writer[capacity];

            for (int i = 0; i < capacity; i++)
            {
                readers[i] = NullRead;
                writers[i] = NullWrite;
            }
        }

        private static void NullRead(ushort address, ref byte data)
        {
        }

        private static void NullWrite(ushort address, ref byte data)
        {
        }

        public void Map(string pattern, Reader reader = null, Writer writer = null)
        {
            var min = BitString.Min(pattern);
            var max = BitString.Max(pattern);
            var mask = BitString.Mask(pattern);

            for (var address = min; address <= max; address++)
            {
                if ((address & mask) == min)
                {
                    readers[address] = reader ?? readers[address];
                    writers[address] = writer ?? writers[address];
                }
            }
        }

        public void Read(ushort address, ref byte data)
        {
            readers[address](address, ref data);
        }

        public void Write(ushort address, ref byte data)
        {
            writers[address](address, ref data);
        }
    }
}
