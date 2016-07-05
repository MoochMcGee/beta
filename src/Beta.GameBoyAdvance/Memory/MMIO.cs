using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Exceptions;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class Mmio
    {
        private const uint SIZE = (1u << 10);
        private const uint MASK = (SIZE - 1u);

        private Reader[] peek = new Reader[SIZE];
        private Writer[] poke = new Writer[SIZE];
        private Register32 latch;

        public uint Peek(int size, uint address)
        {
            if (address > 0x040003ff)
            {
                return 0;
            }

            address = (address & 0x3ff);

            latch.ub0 = peek[address | 0](address | 0); if (size == 0) return latch.ub0;
            latch.ub1 = peek[address | 1](address | 1); if (size == 1) return latch.uw0;
            latch.ub2 = peek[address | 2](address | 2);
            latch.ub3 = peek[address | 3](address | 3); if (size == 2) return latch.ud0;

            throw new CompilerPleasingException();
        }

        public void Poke(int size, uint address, uint data)
        {
            if (address > 0x040003ff)
            {
                return;
            }

            address = address & MASK;

            poke[address | 0](address | 0, (byte)(data >> 0)); if (size == 0) return;
            poke[address | 1](address | 1, (byte)(data >> 8)); if (size == 1) return;
            poke[address | 2](address | 2, (byte)(data >> 16));
            poke[address | 3](address | 3, (byte)(data >> 24)); if (size == 2) return;

            throw new CompilerPleasingException();
        }

        public void Map(uint address, Reader peekFunction)
        {
            peek[address] = peekFunction;
        }

        public void Map(uint address, Writer pokeFunction)
        {
            poke[address] = pokeFunction;
        }

        public void Map(uint address, Reader peekFunction, Writer pokeFunction)
        {
            Map(address, peekFunction);
            Map(address, pokeFunction);
        }

        public void Map(uint address, uint last, Reader peekFunction, Writer pokeFunction)
        {
            for (; address <= last; address++)
            {
                Map(address, peekFunction, pokeFunction);
            }
        }
    }
}
