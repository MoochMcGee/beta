namespace Beta.Platform
{
    public static class BitString
    {
        public static uint Mask(string pattern, int index = 0, uint value = 0)
        {
            if (index == pattern.Length)
            {
                return value;
            }
            else
            {
                switch (pattern[index])
                {
                case ' ': return Mask(pattern, index + 1, (value << 0) | 0);
                case '0': return Mask(pattern, index + 1, (value << 1) | 1);
                case '1': return Mask(pattern, index + 1, (value << 1) | 1);
                default : return Mask(pattern, index + 1, (value << 1) | 0);
                }
            }
        }

        public static uint Test(string pattern, int index = 0, uint value = 0)
        {
            if (index == pattern.Length)
            {
                return value;
            }
            else
            {
                switch (pattern[index])
                {
                case ' ': return Test(pattern, index + 1, (value << 0) | 0);
                case '0': return Test(pattern, index + 1, (value << 1) | 0);
                case '1': return Test(pattern, index + 1, (value << 1) | 1);
                default : return Test(pattern, index + 1, (value << 1) | 0);
                }
            }
        }
    }
}
