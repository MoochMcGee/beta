namespace Beta.GameBoy.Memory
{
    public sealed class MMIO
    {
        private readonly Registers regs;

        public MMIO(IAddressSpace addressSpace, Registers regs)
        {
            this.regs = regs;

            addressSpace.Map(0xff04, 0xff07, Read, Write);
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff04: return regs.tma.divider;
            case 0xff05: return regs.tma.counter;
            case 0xff06: return regs.tma.modulus;
            case 0xff07: return regs.tma.control;
            }

            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            switch (address)
            {
            case 0xff04: regs.tma.divider = 0x00; break;
            case 0xff05: regs.tma.counter = data; break;
            case 0xff06: regs.tma.modulus = data; break;
            case 0xff07: regs.tma.control = data; break;
            }
        }
    }
}
