using System.IO;
using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.GameBoyAdvance.PPU;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Video;
using word = System.UInt32;

namespace Beta.GameBoyAdvance
{
    public partial class Driver : IDriver
    {
        private GamePak gamePak;

        public Apu Apu;
        public Cpu Cpu;
        public Ppu Ppu;
        public Pad Pad;

        public Driver(
            IAudioBackend audio,
            IVideoBackend video,
            ERAM eram,
            IRAM iram,
            MMIO mmio,
            ORAM oram,
            PRAM pram,
            VRAM vram,
            IProducer<ClockSignal> clock,
            IProducer<FrameSignal> frame,
            IProducer<InterruptSignal> interrupt,
            IProducer<HBlankSignal> hblank,
            IProducer<VBlankSignal> vblank)
        {
            this.eram = eram;
            this.iram = iram;
            this.mmio = mmio;
            this.oram = oram;
            this.pram = pram;
            this.vram = vram;

            Cpu = new Cpu(this, mmio, clock, interrupt);
            Ppu = new Ppu(mmio, oram, pram, vram, frame, interrupt, hblank, vblank, video);
            Apu = new Apu(this, mmio, audio);
            Pad = new Pad(mmio);

            clock.Subscribe(Ppu);
            clock.Subscribe(Apu);
            clock.Subscribe(Cpu.Timer);

            interrupt.Subscribe(Cpu);
            hblank.Subscribe(Cpu.Dma);
            vblank.Subscribe(Cpu.Dma);

            frame.Subscribe(Pad);

            mmio.Map(0x204, Write204);
            mmio.Map(0x205, Write205);
            // 20a - 20f
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

        public void Main()
        {
            Cpu.Initialize();
            Apu.Initialize();

            gamePak.Initialize();

            while (true)
            {
                Cpu.Update();
            }
        }

        public void LoadGame(byte[] binary)
        {
            bios = new BIOS(Cpu, File.ReadAllBytes("drivers/gba.sys/bios.rom"));
            gamePak = new GamePak(this, binary);
        }
    }
}
