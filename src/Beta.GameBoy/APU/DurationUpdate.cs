namespace Beta.GameBoy.APU
{
    public static class DurationUpdate
    {
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
