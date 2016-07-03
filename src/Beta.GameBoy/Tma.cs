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
        private readonly TmaRegisters regs;

        public Tma(Registers regs, IProducer<InterruptSignal> ints)
        {
            this.ints = ints;
            this.regs = regs.tma;
        }

        public void Consume(ClockSignal e)
        {
            regs.counter_prescaler += e.Cycles;
            regs.divider_prescaler += e.Cycles;

            if (regs.divider_prescaler >= lut[3])
            {
                regs.divider_prescaler -= lut[3];
                regs.divider++;
            }

            if (regs.counter_prescaler >= lut[regs.control & 3])
            {
                regs.counter_prescaler -= lut[regs.counter & 3];

                if ((regs.control & 4) != 0)
                {
                    regs.counter++;

                    if (regs.counter == 0)
                    {
                        regs.counter = regs.modulus;
                        ints.Produce(new InterruptSignal(Cpu.INT_ELAPSE));
                    }
                }
            }
        }
    }
}
