namespace Beta.Platform
{
    public static class BitString
    {
        private static uint Decode(string pattern, uint capture0, uint capture1, uint dontCare)
        {
            var value = 0U;

            foreach (var character in pattern)
            {
                switch (character)
                {
                case '-': value = (value << 1) | dontCare; break;
                case '0': value = (value << 1) | capture0; break;
                case '1': value = (value << 1) | capture1; break;
                }
            }

            return value;
        }

        public static uint Min(string pattern)
        {
            return Decode(pattern, 0, 1, 0);
        }

        public static uint Max(string pattern)
        {
            return Decode(pattern, 0, 1, 1);
        }

        public static uint Mask(string pattern)
        {
            return Decode(pattern, 1, 1, 0);
        }
    }
}
