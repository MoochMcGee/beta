namespace Beta.Famicom.CPU
{
    public sealed class EnvelopeState
    {
        public int period;
        public int timer;

        public bool constant;
        public bool looping;
        public bool start;
        public int decay;
    }
}
