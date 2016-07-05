using Beta.Platform;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class Registers
    {
        public readonly CpuRegisters cpu = new CpuRegisters();
    }

    public sealed class CpuRegisters
    {
        public Register16 ief;
        public Register16 irf;
        public bool ime;
    }
}
