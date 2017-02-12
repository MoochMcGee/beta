namespace Beta.Platform.Processors.RP6502
{
    public sealed class R6502State
    {
        public Flags flags = new Flags();
        public Interrupts ints = new Interrupts();
        public Registers regs;

        public byte code;
        public ushort address;
        public byte data;
        public bool read;
        public bool ready;
    }
}
