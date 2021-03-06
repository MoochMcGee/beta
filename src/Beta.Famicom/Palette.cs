﻿using System;
using System.Linq;

namespace Beta.Famicom
{
    public static class Palette
    {
        public static readonly int[] Lookup = Generator.generate();

        private static class Generator
        {
            public static int[] generate()
            {
                var linq =
                    from i in Enumerable.Range(0, 64 * 8)
                    select generateColor(i);

                return linq.ToArray();
            }

            private static int generateColor(int pixel)
            {
                const double tau = Math.PI * 2;

                const double black = 0.518;
                const double white = 1.962;
                const double attenuation = 0.746;

                var color = (pixel & 0x0f);
                var level = (color < 0x0e) ? (pixel >> 4) & 3 : 1;

                var levels = new[]
                {
                    0.350, 0.518, 0.962, 1.550,
                    1.090, 1.500, 1.960, 1.960
                };

                var loAndHi = new[]
                {
                    levels[level + (color == 0x0 ? 4 : 0)],
                    levels[level + (color <= 0xc ? 4 : 0)]
                };

                double y = 0;
                double i = 0;
                double q = 0;

                for (var p = 0; p < 12; p++)
                {
                    var spot = loAndHi[wave(p, color) ? 1 : 0];

                    if (((pixel & (1 << 6)) != 0 && wave(p, 0)) ||
                        ((pixel & (1 << 7)) != 0 && wave(p, 4)) ||
                        ((pixel & (1 << 8)) != 0 && wave(p, 8)))
                    {
                        spot *= attenuation;
                    }

                    var v = (spot - black) / (white - black);
                    
                    y += (v / 12);
                    i += (v / 12) * Math.Cos((tau / 12) * p);
                    q += (v / 12) * Math.Sin((tau / 12) * p);
                }

                var r = gamma(y + 0.946882 * i + 0.623557 * q);
                var g = gamma(y - 0.274788 * i - 0.635691 * q);
                var b = gamma(y - 1.108545 * i + 1.709007 * q);

                return (r << 16) | (g << 8) | (b << 0);
            }

            private static bool wave(int phase, int color)
            {
                return (color + phase + 8) % 12 < 6;
            }

            private static byte gamma(double value)
            {
                const double gamma = 2.2 / 1.8;

                if (value < 0)
                {
                    return 0;
                }
                else
                {
                    return clamp(255 * Math.Pow(value, gamma));
                }
            }

            private static byte clamp(double value)
            {
                value = Math.Min(value, 255);
                value = Math.Max(value, 0);

                return ((byte)(value));
            }
        }
    }
}
