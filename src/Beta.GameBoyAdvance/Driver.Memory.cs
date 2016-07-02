using Beta.GameBoyAdvance.Memory;
using Beta.Platform.Exceptions;

namespace Beta.GameBoyAdvance
{
    public partial class Driver
    {
        private static int[][] timingTable = new[]
        {
            //      0, 1, 2, 3, 4, 5, 6, 7, 8, 9, a, b, c, d, e, f
            new[] { 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new[] { 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new[] { 1, 1, 6, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        public Bios bios;
        public Eram eram = new Eram();
        public Iram iram = new Iram();
        public Mmio mmio = new Mmio();
        public Oram oram = new Oram();
        public Pram pram = new Pram();
        public Vram vram = new Vram();

        public uint Read(int size, uint address)
        {
            var area = (address >> 24) & 15;

            Cpu.Cycles += timingTable[size][area];

            switch (area)
            {
            case 0x0:
            case 0x1: return bios.Peek(size, address);
            case 0x2: return eram.Peek(size, address);
            case 0x3: return iram.Peek(size, address);
            case 0x4: return mmio.Peek(size, address);
            case 0x5: return pram.Peek(size, address);
            case 0x6: return vram.Peek(size, address);
            case 0x7: return oram.Peek(size, address);
            case 0x8:
            case 0x9:
            case 0xa:
            case 0xb:
            case 0xc:
            case 0xd: return gamePak.PeekRom(size, address);
            case 0xe:
            case 0xf: return gamePak.PeekRam(size, address);
            }

            throw new CompilerPleasingException();
        }

        public void Write(int size, uint address, uint data)
        {
            var area = (address >> 24) & 15;

            Cpu.Cycles += timingTable[size][area];

            switch (area)
            {
            case 0x0:
            case 0x1: bios.Poke(size, address, data); break;
            case 0x2: eram.Poke(size, address, data); break;
            case 0x3: iram.Poke(size, address, data); break;
            case 0x4: mmio.Poke(size, address, data); break;
            case 0x5: pram.Poke(size, address, data); break;
            case 0x6: vram.Poke(size, address, data); break;
            case 0x7: oram.Poke(size, address, data); break;
            case 0x8:
            case 0x9:
            case 0xa:
            case 0xb:
            case 0xc:
            case 0xd: gamePak.PokeRom(size, address, data); break;
            case 0xe:
            case 0xf: gamePak.PokeRam(size, address, data); break;
            }
        }
    }
}
