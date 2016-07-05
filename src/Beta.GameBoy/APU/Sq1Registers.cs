namespace Beta.GameBoy.APU
{
    public sealed class Sq1Registers
    {
        public bool enabled;
        public int period;
        public int timer = 2048;

        public int duty_form;
        public int duty_step;

        public bool duration_loop;
        public int duration;
        public int duration_latch;

        public int volume;
        public int volume_direction;
        public int volume_period;
        public int volume_timer;

        public bool sweep_enabled;
        public int sweep_direction;
        public int sweep_period;
        public int sweep_timer;
        public int sweep_shift;
    }
}
