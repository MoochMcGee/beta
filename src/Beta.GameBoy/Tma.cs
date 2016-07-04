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

            this.regs.divider_prescaler = lut[3];
            this.regs.counter_prescaler = lut[0];
        }

        public void Consume(ClockSignal e)
        {
            for (int i = 0; i < e.Cycles; i++)
            {
                Tick();
            }
        }

        private void Tick()
        {
            regs.divider_prescaler--;

            if (regs.divider_prescaler == 0)
            {
                regs.divider_prescaler = lut[3];
                regs.divider++;
            }

            regs.counter_prescaler--;

            if (regs.counter_prescaler == 0)
            {
                regs.counter_prescaler = lut[regs.control & 3];

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
