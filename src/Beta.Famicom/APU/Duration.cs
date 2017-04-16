namespace Beta.Famicom.APU
{
    public static class Duration
    {
        static readonly int[] counter_lut =
        {
            10, 254, 20,  2, 40,  4, 80,  6, 160,  8, 60, 10, 14, 12, 26, 14,
            12,  16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };

        public static void tick(DurationState e)
        {
            if (e.halted)
            {
                return;
            }

            if (e.counter != 0)
            {
                e.counter--;
            }
        }

        public static void write(DurationState e, byte data)
        {
            e.counter = counter_lut[data >> 3];
        }
    }
}
