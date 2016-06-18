namespace Beta.Platform.Processors.RP65816
{
    public struct Status
    {
        public bool e;
        public bool n;
        public bool v;
        public bool m;
        public bool x;
        public bool d;
        public bool i;
        public bool z;
        public bool c;

        public byte Pack()
        {
            return (byte)(
                (n ? 0x80 : 0) |
                (v ? 0x40 : 0) |
                (m ? 0x20 : 0) |
                (x ? 0x10 : 0) |
                (d ? 0x08 : 0) |
                (i ? 0x04 : 0) |
                (z ? 0x02 : 0) |
                (c ? 0x01 : 0) | (e ? 0x30 : 0)
            );
        }

        public void Unpack(byte data)
        {
            n = (data & 0x80) != 0;
            v = (data & 0x40) != 0;
            m = (data & 0x20) != 0 || e;
            x = (data & 0x10) != 0 || e;
            d = (data & 0x08) != 0;
            i = (data & 0x04) != 0;
            z = (data & 0x02) != 0;
            c = (data & 0x01) != 0;
        }
    }
}
