namespace Beta.Famicom.CPU
{
    public sealed class Sweep
    {
        public bool enabled;
        public int period;
        public int timer;

        public bool negated;
        public bool reload;
        public int shift;
        public int target;

        public static int Tick(Sweep e, int period, int negate)
        {
            if (e.reload)
            {
                e.reload = false;
                e.timer = e.period;
            }
            else
            {
                if (e.timer != 0)
                {
                    e.timer--;
                }
                else
                {
                    e.timer = e.period;

                    if (e.enabled && e.shift != 0)
                    {
                        e.target = e.negated
                            ? period + (negate >> e.shift)
                            : period + (period >> e.shift)
                            ;

                        if (e.target >= 0 && e.target <= 0x7ff)
                        {
                            return e.target;
                        }
                    }
                }
            }

            return period;
        }
    }
}
