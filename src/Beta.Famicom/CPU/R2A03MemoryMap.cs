using Beta.Famicom.Memory;
using Beta.Famicom.PPU;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public static class R2A03MemoryMap
    {
        static readonly byte[] wram = new byte[0x800];
        
        public static void Read(State e, int address, ref byte data)
        {
            CartridgeConnector.R2A03Read(address, ref data);

            if (address <= 0x1fff)
            {
                data = wram[address & 0x7ff];
            }
            else if (address <= 0x3fff)
            {
                R2C02Registers.Read(e.r2c02, address, ref data);

                Interrupt.nmi(
                    e.r2a03.r6502.ints,
                    e.r2c02.vbl_enabled & e.r2c02.vbl_flag
                );
            }
            else if (address <= 0x4017)
            {
                R2A03Registers.Read(e.r2a03, address, ref data);

                Interrupt.irq(
                    e.r2a03.r6502.ints,
                    e.r2a03.sequence_irq_pending ? 1 : 0);
            }
        }

        public static void Write(State e, int address, byte data)
        {
            CartridgeConnector.R2A03Write(address, data);

            if (address <= 0x1fff)
            {
                wram[address & 0x7ff] = data;
            }
            else if (address <= 0x3fff)
            {
                R2C02Registers.Write(e.r2c02, address, data);

                Interrupt.nmi(
                    e.r2a03.r6502.ints,
                    e.r2c02.vbl_enabled & e.r2c02.vbl_flag
                );
            }
            else if (address <= 0x4017)
            {
                R2A03Registers.Write(e.r2a03, address, data);

                Interrupt.irq(
                    e.r2a03.r6502.ints,
                    e.r2a03.sequence_irq_pending ? 1 : 0);
            }
        }
    }
}
