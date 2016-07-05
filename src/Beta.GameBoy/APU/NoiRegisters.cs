namespace Beta.GameBoy.APU
{
    public sealed class NoiRegisters
    {
        public bool enabled;
        public int period;
        public int timer;

        public bool duration_loop;
        public int duration;
        public int duration_latch;

        public int volume;
        public int volume_latch;
        public int volume_direction;
        public int volume_period;
        public int volume_timer;

        public int lfsr = 0x7fff;
        public int lfsr_mode;
    }
}
