namespace Beta.Famicom.CPU
{
    public sealed class R2A03State
    {
        public bool sequence_irq_enabled = true;
        public bool sequence_irq_pending;
        public int sequence_time;
        public int sequence_mode;

        public int sample_prescaler = 19687500;

        public bool dma_trigger;
        public byte dma_segment;

        public readonly SQ1State sq1 = new SQ1State();
        public readonly SQ2State sq2 = new SQ2State();
        public readonly TRIState tri = new TRIState();
        public readonly NOIState noi = new NOIState();
        public readonly DMCState dmc = new DMCState();
    }
}
