namespace Beta.Famicom.Memory
{
    public sealed class Registers
    {
        public readonly R2A03Registers r2a03 = new R2A03Registers();
        public readonly R2C02Registers r2c02 = new R2C02Registers();
    }

    public sealed class R2A03Registers
    {
    }

    public sealed class R2C02Registers
    {
        public Fetch fetch = new Fetch();
        public byte chr;
        public byte oam_address;
        public byte oam_address_latch;
        public byte[] oam = new byte[256];
        public byte[] pal = new byte[32];
        public bool field;
        public bool obj_overflow;
        public bool obj_zero_hit;
        public int vbl_enabled;
        public int vbl_flag;
        public int vbl_hold;
        public int h;
        public int v = 261;
        public int clipping;
        public int emphasis;

        public sealed class Fetch
        {
            public byte Attr;
            public byte Bit0;
            public byte Bit1;
            public byte Name;
            public ushort Address;
        }
    }
}
