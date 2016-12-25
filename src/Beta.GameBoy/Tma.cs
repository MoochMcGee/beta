using Beta.GameBoy.CPU;
using Beta.GameBoy.Messaging;
using Beta.Platform.Messaging;

namespace Beta.GameBoy
{
    public sealed class Tma
        : IConsumer<ClockSignal>
        , IConsumer<ResetDividerSignal>
    {
        private static readonly int[] lut = new[]
        {
            9, //   4,096 Hz
            3, // 262,144 Hz
            5, //  65,536 Hz
            7  //  16,384 Hz
        };

        private readonly IProducer<InterruptSignal> ints;
        private readonly TmaState regs;

        public Tma(State regs, IProducer<InterruptSignal> ints)
        {
            this.ints = ints;
            this.regs = regs.tma;
        }

        public void Consume(ClockSignal e)
        {
            for (int i = 0; i < e.Cycles; i++)
            {
                Tick();
            }
        }

        public void Consume(ResetDividerSignal e)
        {
            WriteDivider(0);
        }

        private void Tick()
        {
            WriteDivider(regs.divider + 1);
        }

        private void WriteDivider(int next)
        {
            int prev = regs.divider;

            if ((regs.control & 4) != 0)
            {
                int bit = lut[regs.control & 3];
                int prev_bit = (prev >> bit) & 1;
                int next_bit = (next >> bit) & 1;

                if (prev_bit == 1 && next_bit == 0)
                {
                    TickCounter();
                }
            }

            regs.divider = next;
        }

        private void TickCounter()
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
