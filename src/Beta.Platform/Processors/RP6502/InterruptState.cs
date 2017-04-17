namespace Beta.Platform.Processors.RP6502
{
    public sealed class InterruptState
    {
        public int irq;
        public int nmi, nmi_latch;
        public int res;
        public int int_available;
    }
}
