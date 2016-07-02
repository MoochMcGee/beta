using Beta.GameBoy.Memory;

namespace Beta.GameBoy
{
    public delegate byte Reader(ushort address);

    public delegate void Writer(ushort address, byte data);

    public sealed class AddressSpace : IAddressSpace
    {
        private readonly Reader[] readers = new Reader[65536];
        private readonly Writer[] writers = new Writer[65536];

        public AddressSpace(MMIO mmio)
        {
            for (int i = 0; i < 65536; i++)
            {
                readers[i] = NullReader;
                writers[i] = NullWriter;
            }

            Map(0xff00, mmio.Read, mmio.Write);
            Map(0xff04, 0xff07, mmio.Read, mmio.Write);
            Map(0xff0f, mmio.Read, mmio.Write);
            Map(0xffff, mmio.Read, mmio.Write);
        }

        private static byte NullReader(ushort address)
        {
            return 0;
        }

        private static void NullWriter(ushort address, byte data)
        {
        }

        public void Map(ushort start, Reader reader = null, Writer writer = null)
        {
            readers[start] = reader ?? readers[start];
            writers[start] = writer ?? writers[start];
        }

        public void Map(ushort start, ushort end, Reader reader = null, Writer writer = null)
        {
            for (int i = start; i <= end; i++)
            {
                readers[i] = reader ?? readers[i];
                writers[i] = writer ?? writers[i];
            }
        }

        public byte Read(ushort address)
        {
            return readers[address](address);
        }

        public void Write(ushort address, byte data)
        {
            writers[address](address, data);
        }
    }
}
