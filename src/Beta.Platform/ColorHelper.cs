namespace Beta.Platform
{
    public static class ColorHelper
    {
        public static int FromRGB(byte r, byte g, byte b)
        {
            const byte a = 255;

            return (a << 24) | (r << 16) | (g << 8) | (b << 0);
        }
    }
}
