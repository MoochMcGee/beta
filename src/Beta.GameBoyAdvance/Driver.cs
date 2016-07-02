using System.IO;
using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.PPU;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;
using word = System.UInt32;

namespace Beta.GameBoyAdvance
{
    public partial class Driver : IDriver
    {
        private byte[] ioMemory = new byte[1024];

        private GamePak gamePak;

        public Apu Apu;
        public Cpu Cpu;
        public Ppu Ppu;
        public Pad Pad;

        public IAudioBackend Audio { get; set; }

        public IVideoBackend Video { get; set; }

        public Driver()
        {
            Cpu = new Cpu(this);
            Ppu = new Ppu(this);
            Apu = new Apu(this);
            Pad = new Pad(this);
        }

        private byte PeekOpenBus(word address)
        {
            return ioMemory[address & 0x3ff];
        }

        private void PokeOpenBus(word address, byte data)
        {
            ioMemory[address & 0x3ff] = data;
        }

        private void Poke204(word address, byte data)
        {
            ioMemory[0x204] = data;

            gamePak.SetRamAccessTiming((data >> 0) & 3);
            gamePak.Set1stAccessTiming((data >> 2) & 3, GamePak.WAIT_STATE_0);
            gamePak.Set2ndAccessTiming((data >> 4) & 1, GamePak.WAIT_STATE_0);
            gamePak.Set1stAccessTiming((data >> 5) & 3, GamePak.WAIT_STATE_1);
            gamePak.Set2ndAccessTiming((data >> 7) & 1, GamePak.WAIT_STATE_1);
        }

        private void Poke205(word address, byte data)
        {
            ioMemory[0x205] = data;

            gamePak.Set1stAccessTiming((data >> 0) & 3, GamePak.WAIT_STATE_2);
            gamePak.Set2ndAccessTiming((data >> 2) & 1, GamePak.WAIT_STATE_2);

            //  3-4  PHI Terminal Output        (0..3 = Disable, 4.19MHz, 8.38MHz, 16.78MHz)
            //  5    Not used
            //  6    Game Pak Prefetch Buffer (Pipe) (0=Disable, 1=Enable)
            //  7    Game Pak Type Flag  (Read Only) (0=GBA, 1=CGB) (IN35 signal)
        }

        public void Main()
        {
            Initialize();

            while (true)
            {
                Cpu.Update();
            }
        }

        public void Initialize()
        {
            mmio.Map(0x000, 0x3ff, PeekOpenBus, PokeOpenBus);

            Cpu.Initialize();
            Ppu.Initialize();
            Apu.Initialize();
            Pad.Initialize();

            gamePak.Initialize();

            mmio.Map(0x204, Poke204);
            mmio.Map(0x205, Poke205);
            // 20a - 20f
        }

        public void LoadGame(byte[] binary)
        {
            bios = new Bios(Cpu, File.ReadAllBytes("systems/agb.sys/bios.rom"));
            gamePak = new GamePak(this, binary);
        }
    }
}
