using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.GameBoyAdvance.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public partial class Driver : IDriver
    {
        private readonly IMemoryMap memory;
        private readonly IProducer<ClockSignal> clock;
        private readonly Apu apu;
        private readonly Cpu cpu;
        private readonly Pad pad;
        private readonly Ppu ppu;

        public Driver(
            DmaController dma,
            TimerController timer,
            Apu apu,
            Cpu cpu,
            Pad pad,
            Ppu ppu,
            IMemoryMap memory,
            IProducer<ClockSignal> clock,
            ISubscriptionBroker broker)
        {
            this.memory = memory;
            this.clock = clock;
            this.apu = apu;
            this.pad = pad;
            this.ppu = ppu;
            this.cpu = cpu;

            broker.Subscribe(ppu);
            broker.Subscribe(apu);
            broker.Subscribe(timer);

            broker.Subscribe(cpu);
            broker.Subscribe<HBlankSignal>(dma);
            broker.Subscribe<VBlankSignal>(dma);

            broker.Subscribe(pad);
        }

        public void Main()
        {
            cpu.Initialize();
            apu.Initialize();

            while (true)
            {
                cpu.Update();
            }
        }

        public void LoadGame(byte[] binary)
        {
            var gamePak = new GamePak(binary, clock);
            gamePak.Initialize();

            memory.Initialize(cpu, gamePak);
        }
    }
}
