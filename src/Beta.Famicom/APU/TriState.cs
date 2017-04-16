namespace Beta.Famicom.APU
{
    public sealed class TriState
    {
        public bool enabled;
        public int period;
        public int timer = 1;

        public readonly DurationState duration = new DurationState();

        public int step;

        public int linear_counter;
        public int linear_counter_latch;
        public bool linear_counter_control;
        public bool linear_counter_reload;
    }
}
