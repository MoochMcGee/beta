using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class Sq1 : IConsumer<ClockSignal>
    {
        private readonly Sq1State sq1;

        public Sq1(State state)
        {
            this.sq1 = state.r2a03.sq1;
        }

        public void Consume(ClockSignal e)
        {
            sq1.timer--;

            if (sq1.timer == 0)
            {
                sq1.timer = (sq1.period + 1) * 2;
                sq1.duty_step = (sq1.duty_step - 1) & 7;
            }
        }
    }
}
