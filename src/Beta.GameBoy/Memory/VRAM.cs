namespace Beta.GameBoy.Memory
{
    public sealed class VRAM : IMemory
    {
        private readonly byte[] vram = new byte[0x2000];

        public byte Read(ushort address)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.bkg_enabled &&
            //     sequence == ACTIVE_SEQ)
            //     return 0xFF;

            return vram[address & 0x1fff];
        }

        public void Write(ushort address, byte data)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.bkg_enabled &&
            //     sequence == ACTIVE_SEQ)
            //     return;

            vram[address & 0x1fff] = data;
        }
    }
}
