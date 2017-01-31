namespace Beta.GameBoy.APU
{
    public sealed class Duration
    {
        public bool enabled;
        public int counter;

        public static bool Tick(Duration e)
        {
            return e.enabled && e.counter != 0 && --e.counter == 0;
        }
    }
}
