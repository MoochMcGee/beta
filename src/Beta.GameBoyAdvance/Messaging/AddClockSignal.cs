namespace Beta.GameBoyAdvance.Messaging
{
    public sealed class AddClockSignal
    {
        public int Cycles { get; }

        public AddClockSignal(int cycles)
        {
            this.Cycles = cycles;
        }
    }
}
