using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;
using Beta.SuperFamicom.CPU;
using Beta.SuperFamicom.PAD;
using Beta.SuperFamicom.PPU;
using Beta.SuperFamicom.SMP;

namespace Beta.SuperFamicom
{
    public class Driver : IDriver
    {
        public BusA Bus;
        public Dma DMA;
        public Cpu Cpu;
        public Ppu Ppu;
        public Smp Smp;
        public Pad Joypad1;
        public Pad Joypad2;

        public IAudioBackend Audio { get; set; }

        public IVideoBackend Video { get; set; }

        public Driver(IAudioBackend audio, IVideoBackend video)
        {
            this.Audio = audio;
            this.Video = video;

            Ppu = new Ppu(this);
            Smp = new Smp(this);
            Joypad1 = new Pad(0);
            Joypad2 = new Pad(1);
        }

        public void Main()
        {
            Initialize();
            ResetHard();

            while (true)
            {
                Cpu.Update();
            }
        }

        public void Initialize()
        {
            Smp.Initialize();
            Ppu.Initialize();
            Bus.Initialize();
            Cpu.Initialize();
        }

        public void ResetHard() { }

        public void ResetSoft() { }

        public void LoadGame(byte[] binary)
        {
            Bus = new BusA(this, binary);

            Cpu = new Cpu(Bus);
            DMA = new Dma(Bus);
        }
    }
}
