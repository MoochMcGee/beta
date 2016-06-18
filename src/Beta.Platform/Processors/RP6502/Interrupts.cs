using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.RP6502
{
    public struct Interrupts
    {
        private const ushort NMI_VECTOR = 0xfffa;
        private const ushort RES_VECTOR = 0xfffc;
        private const ushort IRQ_VECTOR = 0xfffe;

        public int Irq;
        public int Nmi, NmiLatch;
        public int Res;
        public int Available;

        public void Poll(int i)
        {
            Available = Res | Nmi | (Irq & ~i);
        }

        public ushort GetVector()
        {
            if (Res == 1) { Res = 0; return RES_VECTOR; }
            if (Nmi == 1) { Nmi = 0; return NMI_VECTOR; }
            if (Irq == 1) { Irq = 0; return IRQ_VECTOR; }

            throw new CompilerPleasingException();
        }
    }
}
