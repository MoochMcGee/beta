namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private static class Mixer
        {
            private static short[][][][][] mixTable;
            private static int accum;
            private static int prevX;
            private static int prevY;

            static Mixer()
            {
                mixTable = new short[16][][][][];

                for (var sq1 = 0; sq1 < 16; sq1++)
                {
                    mixTable[sq1] = new short[16][][][];

                    for (var sq2 = 0; sq2 < 16; sq2++)
                    {
                        mixTable[sq1][sq2] = new short[16][][];

                        for (var tri = 0; tri < 16; tri++)
                        {
                            mixTable[sq1][sq2][tri] = new short[16][];

                            for (var noi = 0; noi < 16; noi++)
                            {
                                mixTable[sq1][sq2][tri][noi] = new short[128];

                                for (var dmc = 0; dmc < 128; dmc++)
                                {
                                    var sqr = (95.88 / (8128.0 / (sq1 + sq2) + 100));
                                    var tnd = (159.79 / (1.0 / ((tri / 8227.0) + (noi / 12241.0) + (dmc / 22638.0)) + 100));

                                    mixTable[sq1][sq2][tri][noi][dmc] = (short)((sqr + tnd) * 32767);
                                }
                            }
                        }
                    }
                }
            }

            private static short Filter(int value)
            {
                const int pole = (int)(32767 * (1.0 - 0.9999));

                accum -= prevX;
                prevX = value << 15;
                accum += prevX - prevY * pole;
                prevY = accum >> 15;

                return (short)prevY;
            }

            public static short MixSamples(byte sq1, byte sq2, byte tri, byte noi, byte dmc)
            {
                return Filter(mixTable[sq1][sq2][tri][noi][dmc]);
            }

            public static short MixSamples(byte sq1, byte sq2, byte tri, byte noi, byte dmc, short ext)
            {
                return Filter(
                    (mixTable[sq1][sq2][tri][noi][dmc] + ext) >> 1);
            }
        }
    }
}
