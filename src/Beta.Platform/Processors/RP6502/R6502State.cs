namespace Beta.Platform.Processors.RP6502
{
    public sealed class R6502State
    {
        public FlagState flags = new FlagState();
        public InterruptState ints = new InterruptState();
        public Registers regs;

        public byte code;
        public ushort address;
        public byte data;
        public bool read;
        public bool ready;
    }
}
