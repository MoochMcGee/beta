namespace Beta.GameBoyAdvance.Memory
{
    public sealed class Registers
    {
        public readonly CpuRegisters cpu = new CpuRegisters();
        public readonly PadRegisters pad = new PadRegisters();
    }

    public sealed class CpuRegisters
    {
        public ushort ief;
        public ushort irf;
        public bool ime;
    }

    public sealed class PadRegisters
    {
        public ushort data;
        public ushort mask;
    }
}
