using Beta.GameBoy.Memory;
using Beta.GameBoy.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;

namespace Beta.GameBoy.CPU
{
    public class Cpu : LR35902, IConsumer<InterruptSignal>
    {
        public const byte INT_VBLANK = (1 << 0);
        public const byte INT_STATUS = (1 << 1);
        public const byte INT_ELAPSE = (1 << 2);
        public const byte INT_SERIAL = (1 << 3);
        public const byte INT_JOYPAD = (1 << 4);

        private readonly IProducer<ClockSignal> clock;
        private readonly MemoryMap memory;
        private readonly CpuState cpu;

        public Cpu(State state, MemoryMap memory, IProducer<ClockSignal> clock)
        {
            this.cpu = state.cpu;
            this.memory = memory;
            this.clock = clock;

            Single = 4;
        }

        protected override void Dispatch()
        {
            if (interrupt.ff2 == 1)
            {
                interrupt.ff2 = 0;
                interrupt.ff1 = 1;
            }

            clock.Produce(new ClockSignal(Single));
        }

        protected override byte Read(ushort address)
        {
            Dispatch();

            return memory.Read(address);
        }

        protected override void Write(ushort address, byte data)
        {
            Dispatch();

            memory.Write(address, data);
        }

        public override void Update()
        {
            base.Update();

            var flags = (cpu.irf & cpu.ief) & -interrupt.ff1;
            if (flags != 0)
            {
                interrupt.ff1 = 0;

                if ((flags & 0x01) != 0) { cpu.irf ^= 0x01; Rst(0x40); return; }
                if ((flags & 0x02) != 0) { cpu.irf ^= 0x02; Rst(0x48); return; }
                if ((flags & 0x04) != 0) { cpu.irf ^= 0x04; Rst(0x50); return; }
                if ((flags & 0x08) != 0) { cpu.irf ^= 0x08; Rst(0x58); return; }
                if ((flags & 0x10) != 0) { cpu.irf ^= 0x10; Rst(0x60); return; }
            }
        }

        public void Consume(InterruptSignal e)
        {
            cpu.irf |= e.Flag;

            if ((cpu.ief & e.Flag) != 0)
            {
                Halt = false;

                if (e.Flag == INT_JOYPAD)
                {
                    Stop = false;
                }
            }
        }
    }
}
