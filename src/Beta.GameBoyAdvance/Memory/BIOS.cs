using Beta.GameBoyAdvance.CPU;
using Beta.Platform;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class BIOS : MemoryChip
    {
        private Cpu cpu;
        private word openBus;

        public BIOS(Cpu cpu, byte[] binary)
            : base(binary)
        {
            this.cpu = cpu;
        }

        public word Read(int size, word address)
        {
            if (cpu.GetProgramCursor() < 0x4000 && address < 0x4000)
            {
                if (size == 2) openBus = w[address >> 2];
                if (size == 1) openBus = h[address >> 1];
                if (size == 0) openBus = b[address >> 0];
            }

            return openBus;
        }

        public void Write(int size, word address, word data)
        {
        }
    }
}
