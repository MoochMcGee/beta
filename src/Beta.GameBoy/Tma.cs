using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.Messaging;
using Beta.Platform.Messaging;

namespace Beta.GameBoy
{
    public sealed class Tma : IConsumer<ClockSignal>
    {
        private static int[] lut = new[]
        {
            0x400, // (4,194,304 Hz / 1024) =   4,096 Hz
            0x010, // (4,194,304 Hz /   16) = 262,144 Hz
            0x040, // (4,194,304 Hz /   64) =  65,536 Hz
            0x100  // (4,194,304 Hz /  256) =  16,384 Hz
        };

        private readonly IProducer<InterruptSignal> ints;
        private readonly Registers regs;

        public Tma(Registers regs, IProducer<InterruptSignal> ints)
        {
            this.ints = ints;
            this.regs = regs;
        }

        public void Consume(ClockSignal e)
        {
            regs.tma.counter_prescaler += e.Cycles;
            regs.tma.divider_prescaler += e.Cycles;

            if (regs.tma.divider_prescaler >= lut[3])
            {
                regs.tma.divider_prescaler -= lut[3];
                regs.tma.divider++;
            }

            if (regs.tma.counter_prescaler >= lut[regs.tma.control & 3])
            {
                regs.tma.counter_prescaler -= lut[regs.tma.counter & 3];

                if ((regs.tma.control & 4) != 0)
                {
                    regs.tma.counter++;

                    if (regs.tma.counter == 0)
                    {
                        regs.tma.counter = regs.tma.modulus;
                        ints.Produce(new InterruptSignal(Cpu.INT_ELAPSE));
                    }
                }
            }
        }
    }
}
