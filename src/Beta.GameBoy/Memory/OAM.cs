namespace Beta.GameBoy.Memory
{
    public sealed class OAM
    {
        private readonly byte[] oam = new byte[0xa0];

        public byte Read(ushort address)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.obj_enabled &&
            //     sequence >= SPRITE_SEQ)
            //     return 0xFF;

            return oam[address & 0xff];
        }

        public void Write(ushort address, byte data)
        {
            // if (regs.ppu.lcd_enabled &&
            //     regs.ppu.obj_enabled &&
            //     sequence >= SPRITE_SEQ)
            //     return;

            oam[address & 0xff] = data;
        }
    }
}
