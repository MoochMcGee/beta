namespace Beta.GameBoy.APU
{
    public sealed class WavState
    {
        public bool enabled;
        public int period;
        public int timer;

        public Duration duration = new Duration();

        public bool dac_power;
        public int volume_code;
        public int volume_shift = 4;

        public byte wave_ram_sample;
        public int wave_ram_cursor;
        public int wave_ram_output;
        public int wave_ram_shift;

        public byte[] regs = new byte[5];
    }
}
