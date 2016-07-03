using System.Diagnostics;

namespace Beta.Platform
{
    public static class ColorHelper
    {
        public static int FromRGB(int r, int g, int b)
        {
            Debug.Assert(r >= 0 && r <= 255);
            Debug.Assert(g >= 0 && g <= 255);
            Debug.Assert(b >= 0 && b <= 255);

            const int a = 255;

            return (a << 24) | (r << 16) | (g << 8) | (b << 0);
        }
    }
}
