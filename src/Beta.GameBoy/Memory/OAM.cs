namespace Beta.GameBoy.Memory
{
    public static class OAM
    {
        public static byte Read(State state, ushort address)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.obj_enabled &&
            //     sequence >= SPRITE_SEQ)
            //     return 0xFF;

            return state.oam[address & 0xff];
        }

        public static void Write(State state, ushort address, byte data)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.obj_enabled &&
            //     sequence >= SPRITE_SEQ)
            //     return;

            state.oam[address & 0xff] = data;
        }
    }
}
