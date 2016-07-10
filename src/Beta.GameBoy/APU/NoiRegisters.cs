namespace Beta.GameBoy.APU
{
    public sealed class NoiRegisters
    {
        public bool enabled;
        public int period;
        public int timer;

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();

        public int lfsr = 0x7fff;
        public int lfsr_mode;
    }
}
