using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;

namespace Beta.GameBoyAdvance.CPU
{
    public class Cpu : Arm7, IConsumer<InterruptSignal>
    {
        private readonly IMemoryMap memory;
        private readonly IProducer<ClockSignal> clock;
        private readonly DmaController dma;
        private readonly CpuRegisters regs;
        private readonly TimerController timer;

        public Cpu(Registers regs, IMemoryMap memory, DmaController dma, TimerController timer, IProducer<ClockSignal> clock)
        {
            this.regs = regs.cpu;
            this.memory = memory;
            this.clock = clock;
            this.dma = dma;
            this.timer = timer;
        }

        protected override void Dispatch()
        {
            dma.Transfer();

            clock.Produce(new ClockSignal(Cycles));
        }

        protected override uint Read(int size, uint address)
        {
            return memory.Read(size, address);
        }

        protected override void Write(int size, uint address, uint data)
        {
            if (size == 0)
            {
                data = (data & 0xff);
                data |= (data << 8);
            }

            if (size == 1)
            {
                data = (data & 0xffff);
                data |= (data << 16);
            }

            memory.Write(size, address, data);
        }

        public override void Initialize()
        {
            base.Initialize();

            timer.Initialize();
        }

        public override void Update()
        {
            interrupt = ((regs.ief.w & regs.irf.w) != 0) && regs.ime;

            base.Update();
        }

        public void Consume(InterruptSignal e)
        {
            regs.irf.w |= e.Flag;
        }

        public static class Source
        {
            public const ushort VBlank = 0x0001; // 0 - lcd v-blank
            public const ushort HBlank = 0x0002; // 1 - lcd h-blank
            public const ushort VCheck = 0x0004; // 2 - lcd v-counter match
            public const ushort Timer0 = 0x0008; // 3 - timer 0 overflow
            public const ushort Timer1 = 0x0010; // 4 - timer 1 overflow
            public const ushort Timer2 = 0x0020; // 5 - timer 2 overflow
            public const ushort Timer3 = 0x0040; // 6 - timer 3 overflow
            public const ushort Serial = 0x0080; // 7 - serial communication
            public const ushort Dma0   = 0x0100; // 8 - dma 0
            public const ushort Dma1   = 0x0200; // 9 - dma 1
            public const ushort Dma2   = 0x0400; // a - dma 2
            public const ushort Dma3   = 0x0800; // b - dma 3
            public const ushort Joypad = 0x1000; // c - keypad
            public const ushort Cart   = 0x2000; // d - game pak
            public const ushort Res0   = 0x4000; // e - not used
            public const ushort Res1   = 0x8000; // f - not used
        }
    }
}
