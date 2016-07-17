using static System.Math;

namespace Beta.GameBoy.APU
{
    public static class EnvelopeManager
    {
        public static void Tick(Envelope e)
        {
            if (e.period == 0 || e.timer == 0)
            {
                return;
            }

            e.timer--;

            if (e.timer == 0)
            {
                e.timer = e.period;
                e.count = e.direction == 0
                    ? Max(e.count - 1, 0x0)
                    : Min(e.count + 1, 0xf)
                    ;
            }
        }
    }
}
