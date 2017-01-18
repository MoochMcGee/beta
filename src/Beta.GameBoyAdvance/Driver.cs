using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.GameBoyAdvance.PPU;
using Beta.Platform;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public partial class Driver : IDriver
    {
        private readonly MemoryMap memory;
        private readonly Apu apu;
        private readonly Cpu cpu;
        private readonly Pad pad;
        private readonly Ppu ppu;
        private readonly IProducer<AddClockSignal> clock;

        public Driver(
            DmaController dma,
            TimerController timer,
            Apu apu,
            Cpu cpu,
            Pad pad,
            Ppu ppu,
            MemoryMap memory,
            IProducer<AddClockSignal> clock,
            ISignalBroker broker)
        {
            this.memory = memory;
            this.clock = clock;
            this.apu = apu;
            this.pad = pad;
            this.ppu = ppu;
            this.cpu = cpu;

            broker.Link(ppu);
            broker.Link(apu);
            broker.Link(timer);

            broker.Link<AddClockSignal>(cpu);
            broker.Link<InterruptSignal>(cpu);
            broker.Link<HBlankSignal>(dma);
            broker.Link<VBlankSignal>(dma);

            broker.Link(pad);
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
