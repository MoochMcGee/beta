namespace Beta.GameBoy.Memory
{
    public sealed class MMIO : IMemory
    {
        private readonly Registers regs;

        public MMIO(Registers regs)
        {
            this.regs = regs;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff00:
                if (regs.pad.p15) return regs.pad.p15_latch;
                if (regs.pad.p14) return regs.pad.p14_latch;
                return 0xff;

            case 0xff04: return regs.tma.divider;
            case 0xff05: return regs.tma.counter;
            case 0xff06: return regs.tma.modulus;
            case 0xff07: return regs.tma.control;

            case 0xff0f: return regs.cpu.irf;

            case 0xffff: return regs.cpu.ief;
            }

            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            switch (address)
            {
            case 0xff00:
                regs.pad.p15 = (data & 0x20) == 0;
                regs.pad.p14 = (data & 0x10) == 0;
                break;

            case 0xff04: regs.tma.divider = 0x00; break;
            case 0xff05: regs.tma.counter = data; break;
            case 0xff06: regs.tma.modulus = data; break;
            case 0xff07: regs.tma.control = data; break;

            case 0xff0f: regs.cpu.irf = data; break;

            case 0xff50: regs.boot_rom_enabled = false; break;

            case 0xffff: regs.cpu.ief = data; break;
            }
        }
    }
}
