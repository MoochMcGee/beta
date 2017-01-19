namespace Beta.Platform
{
    public static class MathHelper
    {
        private static int GreatestCommonFactor(int a, int b)
        {
            while (b != 0)
            {
                var remainder = (a % b);
                a = b;
                b = remainder;
            }

            return a;
        }

        public static void Reduce(ref int a, ref int b)
        {
            var gcf = GreatestCommonFactor(a, b);

            a /= gcf;
            b /= gcf;
        }

        public static uint SignExtend(int bits, uint number)
        {
            var mask = (1U << bits) - 1;
            var sign = 1U << (bits - 1);

            return ((number & mask) ^ sign) - sign;
        }

        public static uint NextPowerOfTwo(uint number)
        {
            number--;
            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;
            number++;

            return number;
        }
    }
}
