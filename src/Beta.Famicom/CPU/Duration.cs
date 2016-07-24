namespace Beta.Famicom.CPU
{
    public sealed class Duration
    {
        public static readonly int[] duration_lut = new[]
        {
            10,254, 20,  2, 40,  4, 80,  6, 160,  8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };

        public bool halted;
        public int counter;
        public int latch;

        public static void Tick(Duration duration)
        {
            if (duration.counter != 0 && !duration.halted)
            {
                duration.counter--;
            }
        }
    }
}
