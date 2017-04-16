namespace Beta.Famicom.APU
{
    public static class Envelope
    {
        public static void tick(EnvelopeState e)
        {
            if (e.start)
            {
                e.start = false;
                e.timer = e.period + 1;
                e.decay = 15;
            }
            else
            {
                if (e.timer == 0)
                {
                    e.timer = e.period + 1;

                    if (e.decay != 0 || e.looping)
                    {
                        e.decay = (e.decay - 1) & 15;
                    }
                }
                else
                {
                    e.timer--;
                }
            }
        }

        public static int volume(EnvelopeState e)
        {
            if (e.constant)
            {
                return e.period;
            }
            else
            {
                return e.decay;
            }
        }
    }
}
