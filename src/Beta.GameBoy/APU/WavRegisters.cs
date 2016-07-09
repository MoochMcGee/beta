namespace Beta.GameBoy.APU
{
    public sealed class WavRegisters
    {
        public bool enabled;
        public int period;
        public int timer = 2048;

        public bool duration_loop;
        public int duration;
        public int duration_latch;

        public int volume_shift;

        public byte wave_ram_sample;
        public int wave_ram_cursor;
        public int wave_ram_shift;
    }
}
