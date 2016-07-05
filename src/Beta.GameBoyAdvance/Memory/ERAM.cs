using Beta.Platform;
using Beta.Platform.Exceptions;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class ERAM : MemoryChip, IMemory
    {
        private const int SIZE = (1 << 18);
        private const int MASK = (SIZE - 1);

        public ERAM()
            : base(SIZE)
        {
        }

        public word Read(int size, word address)
        {
            if (size == 2) return w[(address & MASK) >> 2];
            if (size == 1) return h[(address & MASK) >> 1];
            if (size == 0) return b[(address & MASK) >> 0];
            throw new CompilerPleasingException();
        }

        public void Write(int size, word address, word data)
        {
            if (size == 0) { b[(address & MASK) >> 0] = (byte)data; return; }
            if (size == 1) { h[(address & MASK) >> 1] = (half)data; return; }
            if (size == 2) { w[(address & MASK) >> 2] = (word)data; return; }
        }
    }
}
