using Beta.Platform;

namespace Beta.GameBoy.Memory
{
    public sealed class Registers
    {
        public TmaRegisters tma = new TmaRegisters();
    }

    public sealed class TmaRegisters
    {
        public byte divider;
        public byte counter;
        public byte control;
        public byte modulus;

        public int divider_prescaler;
        public int counter_prescaler;
    }
}
