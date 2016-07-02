namespace Beta.Platform.Messaging
{
    public sealed class ClockSignal
    {
        public int Cycles { get; }

        public ClockSignal(int cycles)
        {
            this.Cycles = cycles;
        }
    }
}
