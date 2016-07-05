using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Exceptions;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class MMIO : IMemory
    {
        private const uint SIZE = (1u << 10);
        private const uint MASK = (SIZE - 1u);

        private Reader[] readers = new Reader[SIZE];
        private Writer[] writers = new Writer[SIZE];
        private Register32 latch;

        public uint Read(int size, uint address)
        {
            if (address > 0x040003ff)
            {
                return 0;
            }

            address = (address & 0x3ff);

            latch.ub0 = readers[address | 0](address | 0); if (size == 0) return latch.ub0;
            latch.ub1 = readers[address | 1](address | 1); if (size == 1) return latch.uw0;
            latch.ub2 = readers[address | 2](address | 2);
            latch.ub3 = readers[address | 3](address | 3); if (size == 2) return latch.ud0;

            throw new CompilerPleasingException();
        }

        public void Write(int size, uint address, uint data)
        {
            if (address > 0x040003ff)
            {
                return;
            }

            address = address & MASK;

            writers[address | 0](address | 0, (byte)(data >> 0)); if (size == 0) return;
            writers[address | 1](address | 1, (byte)(data >> 8)); if (size == 1) return;
            writers[address | 2](address | 2, (byte)(data >> 16));
            writers[address | 3](address | 3, (byte)(data >> 24)); if (size == 2) return;

            throw new CompilerPleasingException();
        }

        public void Map(uint address, Reader reader)
        {
            readers[address] = reader;
        }

        public void Map(uint address, Writer writer)
        {
            writers[address] = writer;
        }

        public void Map(uint address, Reader reader, Writer writer)
        {
            Map(address, reader);
            Map(address, writer);
        }

        public void Map(uint address, uint last, Reader reader, Writer writer)
        {
            for (; address <= last; address++)
            {
                Map(address, reader, writer);
            }
        }
    }
}
