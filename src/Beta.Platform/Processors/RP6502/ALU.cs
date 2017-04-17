namespace Beta.Platform.Processors.RP6502
{
    public static class ALU
    {
        public static int c;
        public static int v;

        public static byte add(byte a, byte b, int carry = 0)
        {
            var temp = (byte)((a + b) + carry);
            var bits = (byte)((a ^ temp) & ~(a ^ b));

            c = (bits ^ a ^ b ^ temp) >> 7;
            v = (bits) >> 7;

            return temp;
        }

        public static byte sub(byte a, byte b, int carry = 1)
        {
            b ^= 0xff;
            return add(a, b, carry);
        }
    }
}
