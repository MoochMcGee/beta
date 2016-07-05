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

        public BIOS bios;
        public ERAM eram = new ERAM();
        public IRAM iram = new IRAM();
        public MMIO mmio = new MMIO();
        public ORAM oram = new ORAM();
        public PRAM pram = new PRAM();
        public VRAM vram = new VRAM();

        public uint Read(int size, uint address, out int cycles)
        {
            var area = (address >> 24) & 15;

            cycles = timingTable[size][area];

            switch (area)
            {
            case 0x0:
            case 0x1: return bios.Read(size, address);
            case 0x2: return eram.Read(size, address);
            case 0x3: return iram.Read(size, address);
            case 0x4: return mmio.Read(size, address);
            case 0x5: return pram.Read(size, address);
            case 0x6: return vram.Read(size, address);
            case 0x7: return oram.Read(size, address);
            case 0x8:
            case 0x9:
            case 0xa:
            case 0xb:
            case 0xc:
            case 0xd: return gamePak.ReadRom(size, address);
            case 0xe:
            case 0xf: return gamePak.ReadRam(size, address);
            }

            throw new CompilerPleasingException();
        }

        public void Write(int size, uint address, uint data, out int cycles)
        {
            var area = (address >> 24) & 15;

            cycles = timingTable[size][area];

            switch (area)
            {
            case 0x0:
            case 0x1: bios.Write(size, address, data); break;
            case 0x2: eram.Write(size, address, data); break;
            case 0x3: iram.Write(size, address, data); break;
            case 0x4: mmio.Write(size, address, data); break;
            case 0x5: pram.Write(size, address, data); break;
            case 0x6: vram.Write(size, address, data); break;
            case 0x7: oram.Write(size, address, data); break;
            case 0x8:
            case 0x9:
            case 0xa:
            case 0xb:
            case 0xc:
            case 0xd: gamePak.WriteRom(size, address, data); break;
            case 0xe:
            case 0xf: gamePak.WriteRam(size, address, data); break;
            }
        }
    }
}
