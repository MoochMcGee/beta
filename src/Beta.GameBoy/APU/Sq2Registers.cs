namespace Beta.GameBoy.APU
{
    public sealed class Sq2Registers
    {
        public bool enabled;
        public int period;
        public int timer = 2048;

        public int duty_form;
        public int duty_step;

        public int volume;
        public int volume_direction;
        public int volume_period;
        public int volume_timer;

        public bool duration_loop;
        public int duration;
        public int duration_latch;

        public long sample_buffer;
    }
}
