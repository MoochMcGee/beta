using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.RP6502
{
    public static class Interrupt
    {
        const ushort nmiVector = 0xfffa;
        const ushort resVector = 0xfffc;
        const ushort irqVector = 0xfffe;

        public static void irq(InterruptState e, int value)
        {
            e.irq = value; // level sensitive
        }

        public static void nmi(InterruptState e, int value)
        {
            if (e.nmi_latch < value) // edge sensitive (0 -> 1)
            {
                e.nmi = 1;
            }

            e.nmi_latch = value;
        }

        public static void poll(InterruptState e, int i)
        {
            e.int_available = e.res | e.nmi | (e.irq & ~i);
        }

        public static ushort getVector(InterruptState e)
        {
            if (e.res == 1) { e.res = 0; return resVector; }
            if (e.nmi == 1) { e.nmi = 0; return nmiVector; }
            if (e.irq == 1) { e.irq = 0; return irqVector; }

            throw new CompilerPleasingException();
        }
    }
}
