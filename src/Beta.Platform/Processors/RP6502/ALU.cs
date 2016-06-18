namespace Beta.Platform.Processors.RP6502
{
    public static class Alu
    {
        public static int C;
        public static int V;

        public static byte Add(byte a, byte b, int carry = 0)
        {
            var temp = (byte)((a + b) + carry);
            var bits = (byte)((a ^ temp) & ~(a ^ b));

            C = (bits ^ a ^ b ^ temp) >> 7;
            V = (bits) >> 7;

            return temp;
        }

        public static byte Sub(byte a, byte b, int carry = 1)
        {
            b ^= 0xff;
            return Add(a, b, carry);
        }
    }
}
