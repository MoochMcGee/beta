using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class Noi
    {
        private readonly NoiState noi;

        public Noi(State state)
        {
            this.noi = state.r2a03.noi;
        }

        public void Consume(ClockSignal e)
        {
            noi.timer--;

            if (noi.timer == 0)
            {
                noi.timer = noi.period + 1;

                var tap0 = noi.lfsr;
                var tap1 = noi.lfsr_mode == 1
                    ? noi.lfsr >> 6
                    : noi.lfsr >> 1
                    ;

                var feedback = (tap0 ^ tap1) & 1;

                noi.lfsr = (noi.lfsr >> 1) | (feedback << 14);
            }
        }
    }
}
