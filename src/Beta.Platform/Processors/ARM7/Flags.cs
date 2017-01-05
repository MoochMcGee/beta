namespace Beta.Platform.Processors.ARM7
{
    public sealed class Flags
    {
        public uint n, z, c, v;
        public uint r;
        public uint i, f, t, m;

        public void Load(uint value)
        {
            n = (value >> 31) & 1;
            z = (value >> 30) & 1;
            c = (value >> 29) & 1;
            v = (value >> 28) & 1;
            r = (value >> 8) & 0xfffff;
            i = (value >> 7) & 1;
            f = (value >> 6) & 1;
            t = (value >> 5) & 1;
            m = (value >> 0) & 31;
        }

        public uint Save()
        {
            return
                (n << 31) | (z << 30) | (c << 29) | (v << 28) |
                (r << 8) |
                (i << 7) | (f << 6) | (t << 5) | (m << 0);
        }
    }
}
