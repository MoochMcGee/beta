namespace Beta.Famicom.CPU
{
    public sealed class R2A03State
    {
        public int sequence_time;
        public int sequence_mode;

        public int sample_prescaler = 1789772;

        public bool irq_enabled;
        public bool irq_pending;

        public bool dma_trigger;
        public byte dma_segment;

        public readonly Sq1State sq1 = new Sq1State();
        public readonly Sq2State sq2 = new Sq2State();
        public readonly TriState tri = new TriState();
        public readonly NoiState noi = new NoiState();
        public readonly DmcState dmc = new DmcState();
    }
}
