using Beta.Platform;
using Beta.Platform.Exceptions;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class VRAM : MemoryChip, IMemory
    {
        private const int SIZE = (3 << 15);

        public VRAM()
            : base(SIZE)
        {
        }

        private static word MaskAddress(word address)
        {
            switch ((address >> 16) & 1)
            {
            case 0: return (address & 0x0ffff);
            case 1: return (address & 0x17fff);
            }
            throw new CompilerPleasingException();
        }

        public word Read(int size, word address)
        {
            address = MaskAddress(address);

            if (size == 2) return w[address >> 2];
            if (size == 1) return h[address >> 1];
            if (size == 0) return b[address >> 0];
            throw new CompilerPleasingException();
        }

        public void Write(int size, word address, word data)
        {
            address = MaskAddress(address);

            if (size == 2)
            {
                w[address >> 2] = (word)data;
            }
            else
            {
                h[address >> 1] = (half)data;
            }
        }
    }
}
