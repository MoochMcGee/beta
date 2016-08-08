namespace Beta.Famicom.CPU
{
    public sealed class Sq1State
    {
        public bool enabled;
        public int period;
        public int timer = 2;

        public readonly Duration duration = new Duration();
        public readonly Envelope envelope = new Envelope();
        public readonly Sweep sweep = new Sweep();

        public int duty_form;
        public int duty_step;
    }
}
