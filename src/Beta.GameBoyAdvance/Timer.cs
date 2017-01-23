using Beta.GameBoyAdvance.CPU;

namespace Beta.GameBoyAdvance
{
    public sealed class Timer
    {
        public readonly Interrupt interrupt;

        public int control;
        public int counter;
        public int refresh;

        public Timer(Interrupt interrupt)
        {
            this.interrupt = interrupt;
        }
    }
}
