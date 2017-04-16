namespace Beta.Famicom.APU
{
    public sealed class NoiState
    {
        public bool enabled;
        public int period;
        public int timer = 4;

        public readonly DurationState duration = new DurationState();
        public readonly EnvelopeState envelope = new EnvelopeState();

        public int lfsr_mode;
        public int lfsr = 1;
    }
}
