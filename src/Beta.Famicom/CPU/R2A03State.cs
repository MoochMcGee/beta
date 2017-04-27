using Beta.Famicom.APU;
using Beta.R6502;

namespace Beta.Famicom.CPU
{
    public sealed class R2A03State
    {
        public Domain.State r6502 = R6502.State.init;

        public bool sequence_irq_enabled = true;
        public bool sequence_irq_pending;
        public int sequence_time;
        public int sequence_mode;

        public int sample_prescaler = 19687500;

        public bool dma_trigger;
        public byte dma_segment;

        public readonly Sq1State sq1 = new Sq1State();
        public readonly Sq2State sq2 = new Sq2State();
        public readonly TriState tri = new TriState();
        public readonly NoiState noi = new NoiState();
        public readonly DmcState dmc = new DmcState();
    }
}
