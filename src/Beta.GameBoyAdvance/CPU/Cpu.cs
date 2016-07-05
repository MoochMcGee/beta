using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;

namespace Beta.GameBoyAdvance.CPU
{
    public class Cpu : Arm7, IConsumer<InterruptSignal>
    {
        private readonly IMemoryMap memory;
        private readonly IProducer<ClockSignal> clock;

        private Register16 ief;
        private Register16 irf;
        private bool ime;

        public DmaController Dma;
        public TimerController Timer;

        public Cpu(IMemoryMap memory, DmaController dma, TimerController timer, MMIO mmio, IProducer<ClockSignal> clock)
        {
            this.memory = memory;
            this.clock = clock;

            this.Dma = dma;
            this.Timer = timer;

            mmio.Map(0x200, Read200, Write200);
            mmio.Map(0x201, Read201, Write201);
            mmio.Map(0x202, Read202, Write202);
            mmio.Map(0x203, Read203, Write203);
            mmio.Map(0x208, Read208, Write208);
            mmio.Map(0x209, Read209, Write209);
        }

        private byte Read200(uint address)
        {
            return ief.l;
        }

        private byte Read201(uint address)
        {
            return ief.h;
        }

        private byte Read202(uint address)
        {
            return irf.l;
        }

        private byte Read203(uint address)
        {
            return irf.h;
        }

        private void Write200(uint address, byte data)
        {
            ief.l = data;
        }

        private void Write201(uint address, byte data)
        {
            ief.h = data;
        }

        private void Write202(uint address, byte data)
        {
            irf.l &= (byte)~data;
        }

        private void Write203(uint address, byte data)
        {
            irf.h &= (byte)~data;
        }

        private byte Read208(uint address)
        {
            return (byte)(ime ? 1 : 0);
        }

        private byte Read209(uint address)
        {
            return 0;
        }

        private void Write208(uint address, byte data)
        {
            ime = (data & 1) != 0;
        }

        private void Write209(uint address, byte data)
        {
        }

        protected override void Dispatch()
        {
            Dma.Transfer();

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

            Timer.Initialize();
        }

        public override void Update()
        {
            interrupt = ((ief.w & irf.w) != 0) && ime;

            base.Update();
        }

        public void Consume(InterruptSignal e)
        {
            irf.w |= e.Flag;
        }

        public static class Source
        {
            public const ushort V_BLANK = 0x0001; // 0 - lcd v-blank
            public const ushort H_BLANK = 0x0002; // 1 - lcd h-blank
            public const ushort V_CHECK = 0x0004; // 2 - lcd v-counter match
            public const ushort TIMER_0 = 0x0008; // 3 - timer 0 overflow
            public const ushort TIMER_1 = 0x0010; // 4 - timer 1 overflow
            public const ushort TIMER_2 = 0x0020; // 5 - timer 2 overflow
            public const ushort TIMER_3 = 0x0040; // 6 - timer 3 overflow
            public const ushort SERIAL = 0x0080; // 7 - serial communication
            public const ushort DMA_0 = 0x0100; // 8 - dma 0
            public const ushort DMA_1 = 0x0200; // 9 - dma 1
            public const ushort DMA_2 = 0x0400; // a - dma 2
            public const ushort DMA_3 = 0x0800; // b - dma 3
            public const ushort JOYPAD = 0x1000; // c - keypad
            public const ushort CART = 0x2000; // d - game pak
            public const ushort RES0 = 0x4000; // e - not used
            public const ushort RES1 = 0x8000; // f - not used
        }
    }
}
