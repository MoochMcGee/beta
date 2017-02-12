using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.RP6502
{
    public sealed class Interrupts
    {
        const ushort NMI_VECTOR = 0xfffa;
        const ushort RES_VECTOR = 0xfffc;
        const ushort IRQ_VECTOR = 0xfffe;

        public int irq;
        public int nmi, nmi_latch;
        public int res;
        public int int_available;

        public static void IRQ(Interrupts e, int value)
        {
            e.irq = value; // level sensitive
        }

        public static void NMI(Interrupts e, int value)
        {
            if (e.nmi_latch < value) // edge sensitive (0 -> 1)
            {
                e.nmi = 1;
            }

            e.nmi_latch = value;
        }

        public static void Poll(Interrupts e, int i)
        {
            e.int_available = e.res | e.nmi | (e.irq & ~i);
        }

        public static ushort GetVector(Interrupts e)
        {
            if (e.res == 1) { e.res = 0; return RES_VECTOR; }
            if (e.nmi == 1) { e.nmi = 0; return NMI_VECTOR; }
            if (e.irq == 1) { e.irq = 0; return IRQ_VECTOR; }

            throw new CompilerPleasingException();
        }
    }
}
