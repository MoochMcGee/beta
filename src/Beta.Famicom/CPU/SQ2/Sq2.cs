using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class Sq2 : IConsumer<ClockSignal>
    {
        private readonly Sq2State sq2;

        public Sq2(State state)
        {
            this.sq2 = state.r2a03.sq2;
        }

        public void Consume(ClockSignal e)
        {
            sq2.timer--;

            if (sq2.timer == 0)
            {
                sq2.timer = (sq2.period + 1) * 2;
                sq2.duty_step = (sq2.duty_step - 1) & 7;
            }
        }
    }
}
