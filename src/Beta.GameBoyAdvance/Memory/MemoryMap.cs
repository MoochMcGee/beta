using System.IO;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class MemoryMap : IMemoryMap
    {
        private static int[][] timingTable = new[]
        {
            //      0, 1, 2, 3, 4, 5, 6, 7, 8, 9, a, b, c, d, e, f
            new[] { 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new[] { 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new[] { 1, 1, 6, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        private GamePak gamePak;

        private readonly IProducer<AddClockSignal> clock;
        private BIOS bios;
        private readonly ERAM eram;
        private readonly IRAM iram;
        private readonly MMIO mmio;
        private readonly ORAM oram;
        private readonly PRAM pram;
        private readonly VRAM vram;

        public MemoryMap(
            IProducer<AddClockSignal> clock,
            ERAM eram,
            IRAM iram,
            MMIO mmio,
            ORAM oram,
            PRAM pram,
            VRAM vram)
        {
            this.clock = clock;
            this.eram = eram;
            this.iram = iram;
            this.mmio = mmio;
            this.oram = oram;
            this.pram = pram;
            this.vram = vram;

            mmio.Map(0x204, Write204);
            mmio.Map(0x205, Write205);
            // 20a - 20f
        }

        public uint Read(int size, uint address)
        {
            var area = (address >> 24) & 15;

            clock.Produce(new AddClockSignal(timingTable[size][area]));

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

        public void Write(int size, uint address, uint data)
        {
            var area = (address >> 24) & 15;

            clock.Produce(new AddClockSignal(timingTable[size][area]));

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

        public void Initialize(Cpu cpu, GamePak gamePak)
        {
            this.bios = new BIOS(cpu, File.ReadAllBytes("drivers/gba.sys/bios.rom"));
            this.gamePak = gamePak;
        }

        private void Write204(word address, byte data)
        {
            gamePak.SetRamAccessTiming((data >> 0) & 3);
            gamePak.Set1stAccessTiming((data >> 2) & 3, GamePak.WAIT_STATE_0);
            gamePak.Set2ndAccessTiming((data >> 4) & 1, GamePak.WAIT_STATE_0);
            gamePak.Set1stAccessTiming((data >> 5) & 3, GamePak.WAIT_STATE_1);
            gamePak.Set2ndAccessTiming((data >> 7) & 1, GamePak.WAIT_STATE_1);
        }

        private void Write205(word address, byte data)
        {
            gamePak.Set1stAccessTiming((data >> 0) & 3, GamePak.WAIT_STATE_2);
            gamePak.Set2ndAccessTiming((data >> 2) & 1, GamePak.WAIT_STATE_2);

            //  3-4  PHI Terminal Output        (0..3 = Disable, 4.19MHz, 8.38MHz, 16.78MHz)
            //  5    Not used
            //  6    Game Pak Prefetch Buffer (Pipe) (0=Disable, 1=Enable)
            //  7    Game Pak Type Flag  (Read Only) (0=GBA, 1=CGB) (IN35 signal)
        }
    }
}
