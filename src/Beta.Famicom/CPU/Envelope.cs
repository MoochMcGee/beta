namespace Beta.Famicom.CPU
{
    public sealed class Envelope
    {
        public int period;
        public int timer;

        public bool constant;
        public bool looping;
        public bool start;
        public int decay;

        public static void Tick(Envelope envelope)
        {
            if (envelope.start)
            {
                envelope.start = false;
                envelope.timer = envelope.period + 1;
                envelope.decay = 15;
            }
            else
            {
                if (envelope.timer == 0)
                {
                    envelope.timer = envelope.period + 1;

                    if (envelope.decay != 0 || envelope.looping)
                    {
                        envelope.decay = (envelope.decay - 1) & 15;
                    }
                }
                else
                {
                    envelope.timer--;
                }
            }
        }

        public static int Volume(Envelope envelope)
        {
            return envelope.constant
                ? envelope.period
                : envelope.decay
                ;
        }
    }
}
