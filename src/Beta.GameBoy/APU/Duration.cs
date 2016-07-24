namespace Beta.GameBoy.APU
{
    public sealed class Duration
    {
        public bool loop;
        public int count;
        public int latch;

        public static void Tick(Duration e, int count, ref bool enabled)
        {
            if (e.count == 0)
            {
                return;
            }

            e.count--;

            if (e.count == 0)
            {
                if (e.loop)
                {
                    e.count = count - e.latch;
                }
                else
                {
                    enabled = false;
                }
            }
        }
    }
}
