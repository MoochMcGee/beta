namespace Beta.Famicom.CPU
{
    public sealed class TriState
    {
        public bool enabled;
        public int period;
        public int timer = 1;

        public readonly Duration duration = new Duration();

        public int step;
    }
}
