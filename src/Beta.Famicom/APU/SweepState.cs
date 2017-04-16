namespace Beta.Famicom.APU
{
    public sealed class SweepState
    {
        public bool enabled;
        public int period;
        public int timer;

        public bool negated;
        public bool reload;
        public int shift;
        public int target;
    }
}
