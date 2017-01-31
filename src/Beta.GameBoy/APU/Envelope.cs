namespace Beta.GameBoy.APU
{
    public sealed class Envelope
    {
        public int counter;
        public int direction;
        public int latch;
        public int period;
        public int timer;

        public static void Tick(Envelope e)
        {
            if (e.period == 0)
            {
                return;
            }

            if (e.timer != 0 && --e.timer == 0)
            {
                e.timer = e.period;

                if (e.direction == 0 && e.counter > 0x0) { e.counter--; }
                if (e.direction == 1 && e.counter < 0xf) { e.counter++; }
            }
        }
    }
}
