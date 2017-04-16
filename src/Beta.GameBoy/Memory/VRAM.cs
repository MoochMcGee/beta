namespace Beta.GameBoy.Memory
{
    public static class VRAM
    {
        public static byte Read(State state, ushort address)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.bkg_enabled &&
            //     sequence == ACTIVE_SEQ)
            //     return 0xFF;

            return state.vram[address & 0x1fff];
        }

        public static void Write(State state, ushort address, byte data)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.bkg_enabled &&
            //     sequence == ACTIVE_SEQ)
            //     return;

            state.vram[address & 0x1fff] = data;
        }
    }
}
