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
            SignalBroker broker)
        {
            this.memory = memory;
            this.clock = clock;
            this.apu = apu;
            this.pad = pad;
            this.ppu = ppu;
            this.cpu = cpu;

            broker.Link<ClockSignal>(ppu.Consume);
            broker.Link<ClockSignal>(apu.Consume);
            broker.Link<ClockSignal>(timer.Consume);

            broker.Link<AddClockSignal>(cpu.Consume);
            broker.Link<InterruptSignal>(cpu.Consume);
            broker.Link<HBlankSignal>(dma.Consume);
            broker.Link<VBlankSignal>(dma.Consume);

            broker.Link<FrameSignal>(pad.Consume);
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
