namespace Beta.Platform.Processors.RP6502
{
    public sealed class Flags
    {
        public int n;
        public int v;
        public int d;
        public int i;
        public int z;
        public int c;

        public static byte PackFlags(Flags e)
        {
            return (byte)(
                (e.n << 7) |
                (e.v << 6) |
                (e.d << 3) |
                (e.i << 2) |
                (e.z << 1) |
                (e.c << 0) | 0x30);
        }

        public static void UnpackFlags(Flags e, byte value)
        {
            e.n = (value >> 7) & 1;
            e.v = (value >> 6) & 1;
            e.d = (value >> 3) & 1;
            e.i = (value >> 2) & 1;
            e.z = (value >> 1) & 1;
            e.c = (value >> 0) & 1;
        }
    }
}
