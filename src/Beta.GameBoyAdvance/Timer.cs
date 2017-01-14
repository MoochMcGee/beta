namespace Beta.GameBoyAdvance
{
    public sealed class Timer
    {
        public ushort interrupt;
        public int control;
        public int counter;
        public int refresh;

        public Timer(ushort interrupt)
        {
            this.interrupt = interrupt;
        }
    }
}
