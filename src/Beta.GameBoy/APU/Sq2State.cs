namespace Beta.GameBoy.APU
{
    public sealed class Sq2State
    {
        public bool enabled;
        public int period;
        public int timer;

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();

        public bool dac_power;
        public int duty_form;
        public int duty_step;

        public byte[] regs = new byte[5];
    }
}
