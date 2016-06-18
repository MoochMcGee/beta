namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        private static class Mixer
        {
            private static int[] outputs = new int[2];

            public static int[] Flags = new int[2];
            public static int[] Level = new int[2];

            public static int[] MixSamples(int sq1, int sq2, int wav, int noi)
            {
                for (var i = 0; i < 2; i++)
                {
                    var level = 0;
                    var flags = Flags[i];

                    if ((flags & 0x01) != 0) level += sq1;
                    if ((flags & 0x02) != 0) level += sq2;
                    if ((flags & 0x04) != 0) level += wav;
                    if ((flags & 0x08) != 0) level += noi;

                    outputs[i] = (short.MaxValue * (level * (Level[i] + 1))) / (15 * 4 * 8);
                }

                return outputs;
            }
        }
    }
}
