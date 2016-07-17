namespace Beta.Famicom.CPU
{
    public sealed class Sq2State
    {
        public bool enabled;
        public int period;
        public int timer = 1;

        public readonly Duration duration = new Duration();
        public readonly Envelope envelope = new Envelope();

        public int duty_form;
        public int duty_step;
    }
}
