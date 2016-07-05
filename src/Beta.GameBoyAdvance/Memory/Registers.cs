using Beta.Platform;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class Registers
    {
        public readonly CpuRegisters cpu = new CpuRegisters();
        public readonly PadRegisters pad = new PadRegisters();
    }

    public sealed class CpuRegisters
    {
        public Register16 ief;
        public Register16 irf;
        public bool ime;
    }

    public sealed class PadRegisters
    {
        public Register16 data;
        public Register16 mask;
    }
}
