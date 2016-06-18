using System;
using Beta.Platform;

namespace Beta.Famicom
{
    public static class Palette
    {
        public static int[] Ntsc = Generator.Generate();
        public static int[] Palb; // todo: pal-b color generation

        private static class Generator
        {
            private static int GenerateColor(int pixel, float saturation = 1.0f, float hueTweak = 0.0f, float contrast = 1.0f, float brightness = 1.0f, float gamma = 1.8f)
            {
                // The input value is a NES color index (with de-emphasis bits).
                // We need RGB values. Convert the index into RGB.
                // For most part, this process is described at:
                //    http://wiki.nesdev.com/w/index.php/NTSC_video

                // Voltage levels, relative to synch voltage
                const double black = 0.518;
                const double white = 1.962;
                const double attenuation = 0.746;

                // Decode the color index
                var color = (pixel & 0x0f);
                var level = (color < 0x0e) ? (pixel >> 4) & 3 : 1;

                var levels = new[]
                {
                    0.350, 0.518, 0.962, 1.550, // Signal low
                    1.090, 1.500, 1.960, 1.960  // Signal high
                };

                var loAndHi = new[]
                {
                    levels[level + (color == 0x0 ? 4 : 0)],
                    levels[level + (color <  0xD ? 4 : 0)]
                };

                // Calculate the luma and chroma by emulating the relevant circuits:
                double y = 0;
                double i = 0;
                double q = 0;

                for (var p = 0; p < 12; ++p)
                { // 12 clock cycles per pixel.
                    // NES NTSC modulator (square wave between two voltage levels):
                    var spot = loAndHi[Wave(p, color) ? 1 : 0];

                    // De-emphasis bits attenuate a part of the signal:
                    if (((pixel & (1 << 6)) != 0 && Wave(p, 0)) ||
                        ((pixel & (1 << 7)) != 0 && Wave(p, 4)) ||
                        ((pixel & (1 << 8)) != 0 && Wave(p, 8)))
                        spot *= attenuation;

                    // Normalize:
                    var v = (spot - black) / (white - black);

                    // Ideal TV NTSC demodulator:
                    // Apply contrast/brightness
                    v = (v - 0.5f) * contrast + 0.5f;
                    v *= brightness / 12f;

                    y += v;
                    i += v * Math.Cos((MathHelper.Tau / 12f) * (p + hueTweak));
                    q += v * Math.Sin((MathHelper.Tau / 12f) * (p + hueTweak));
                }

                i *= saturation;
                q *= saturation;

                // Convert YIQ into RGB according to FCC-sanctioned conversion matrix.
                return
                    0x010000 * Clamp(255 * GammaFix(y + 0.946882f * i + 0.623557f * q, gamma)) +
                    0x000100 * Clamp(255 * GammaFix(y + -0.274788f * i + -0.635691f * q, gamma)) +
                    0x000001 * Clamp(255 * GammaFix(y + -1.108545f * i + 1.709007f * q, gamma));
            }

            public static int[] Generate()
            {
                var palette = new int[64 * 8];

                for (var i = 0; i < palette.Length; i++)
                {
                    palette[i] = GenerateColor(i, 1.2f);
                }

                return palette;
            }

            private static double GammaFix(double f, double gamma)
            {
                return f < 0.0f ? 0.0f : Math.Pow(f, 2.2f / gamma);
            }

            private static int Clamp(double value)
            {
                return (int)(value < 0 ? 0 : value > 255 ? 255 : value);
            }

            private static bool Wave(int phase, int color)
            {
                return (color + phase + 8) % 12 < 6;
            }
        }
    }
}
