namespace Beta.GameBoy.APU
{
    public sealed class NoiState
    {
        public bool enabled;
        public int period;
        public int timer;

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();

        public bool dac_power;
        public int lfsr = 0x7fff;
        public int lfsr_mode;

        public int lfsr_frequency;
        public int lfsr_divisor;

        public byte[] regs = new byte[5];
    }
}
