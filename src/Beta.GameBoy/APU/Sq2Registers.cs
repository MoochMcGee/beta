namespace Beta.GameBoy.APU
{
    public sealed class Sq2Registers
    {
        public bool enabled;
        public int period;
        public int timer = 2048;

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();

        public int duty_form;
        public int duty_step;
    }
}
