using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.ARM7;

namespace Beta.GameBoyAdvance.CPU
{
    public class Cpu : Core
    {
        private readonly MemoryMap memory;
        private readonly DmaController dma;
        private readonly CpuRegisters regs;
        private readonly TimerController timer;
        private readonly IProducer<ClockSignal> clock;

        public Cpu(Registers regs, MemoryMap memory, DmaController dma, TimerController timer, IProducer<ClockSignal> clock)
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

            clock.Produce(new ClockSignal(cycles));
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

        public override void Update()
        {
            interrupt = ((regs.ief & regs.irf) != 0) && regs.ime;

            base.Update();
        }

        public void Consume(InterruptSignal e)
        {
            regs.irf |= (ushort)e.Flag;
        }

        public void Consume(AddClockSignal e)
        {
            cycles += e.Cycles;
        }
    }
}
