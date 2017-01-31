namespace Beta.GameBoy.APU
{
    public sealed class Sq1State
    {
        public bool enabled;
        public int period;
        public int timer;

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();

        public bool dac_power;
        public int duty_form;
        public int duty_step;

        public bool sweep_enabled;
        public int sweep_direction;
        public int sweep_period;
        public int sweep_timer;
        public int sweep_shift;

        public byte[] regs = new byte[5];
    }
}
