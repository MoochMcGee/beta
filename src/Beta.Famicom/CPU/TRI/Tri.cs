using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class Tri : IConsumer<ClockSignal>
    {
        private readonly TriState tri;

        public Tri(State state)
        {
            this.tri = state.r2a03.tri;
        }

        public void Consume(ClockSignal e)
        {
            tri.timer--;

            if (tri.timer == 0)
            {
                tri.timer = tri.period + 1;
                tri.step = (tri.step + 1) & 31;
            }
        }
    }
}
