namespace Beta.Famicom.PPU
{
    public sealed class R2C02State
    {
        public bool field;
        public bool obj_overflow;
        public bool obj_zero_hit;
        public int vbl_enabled;
        public int vbl_flag;
        public int vbl_hold;
        public int h;
        public int v = 261;
        public int clipping = 0x3f;
        public int emphasis;
        public byte chr;
        public byte oam_address;
        public byte[] oam = new byte[256];

        public bool bkg_clipped = true;
        public bool bkg_enabled;
        public ushort bkg_address;

        public bool obj_clipped = true;
        public bool obj_enabled;
        public ushort obj_address;
        public int obj_rasters = 8;

        public bool scroll_swap;
        public int scroll_fine;
        public ushort scroll_step = 1;
        public ushort scroll_address;
        public ushort scroll_temp;

        public byte fetch_attr;
        public byte fetch_bit0;
        public byte fetch_bit1;
        public byte fetch_name;
        public ushort fetch_address;
    }
}
