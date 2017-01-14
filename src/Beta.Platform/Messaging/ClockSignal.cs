namespace Beta.Platform.Messaging
{
    public sealed class ClockSignal
    {
        public readonly int Cycles;

        public ClockSignal(int cycles)
        {
            this.Cycles = cycles;
        }
    }
}
