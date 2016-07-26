namespace Beta.Famicom.CPU
{
    public sealed class NoiState
    {
        public bool enabled;
        public int period;
        public int timer = 4;

        public readonly Duration duration = new Duration();
        public readonly Envelope envelope = new Envelope();

        public int lfsr_mode;
        public int lfsr = 1;
    }
}
