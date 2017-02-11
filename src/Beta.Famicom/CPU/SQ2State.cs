namespace Beta.Famicom.CPU
{
    public sealed class SQ2State
    {
        public bool enabled;
        public int period;
        public int timer = 2;

        public readonly DurationState duration = new DurationState();
        public readonly EnvelopeState envelope = new EnvelopeState();
        public readonly SweepState sweep = new SweepState();

        public int duty_form;
        public int duty_step;
    }
}
