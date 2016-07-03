namespace Beta.GameBoy.Memory
{
    public sealed class Registers
    {
        public CpuRegisters cpu = new CpuRegisters();
        public PadRegisters pad = new PadRegisters();
        public TmaRegisters tma = new TmaRegisters();

        public bool boot_rom_enabled = true;
    }

    public sealed class CpuRegisters
    {
        public byte ief;
        public byte irf;
    }

    public sealed class PadRegisters
    {
        public bool p14;
        public bool p15;
        public byte p14_latch;
        public byte p15_latch;
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
