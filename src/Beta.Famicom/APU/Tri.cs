namespace Beta.Famicom.APU
{
    public static class Tri
    {
        public static int getOutput(TriState e)
        {
            if (e.period == 0 || e.period == 1)
            {
                return 7;
            }
            else
            {
                return e.step > 0xf
                    ? e.step ^ 0x1f
                    : e.step;
            }
        }

        public static void tick(TriState e)
        {
            e.timer--;

            if (e.timer == 0)
            {
                e.timer = e.period + 1;

                if (e.duration.counter != 0 && e.linear_counter != 0)
                {
                    e.step = (e.step + 1) & 31;
                }
            }
        }

        public static void write(TriState e, int address, byte data)
        {
            switch (address - 0x4008)
            {
            case 0:
                e.duration.halted = (data & 0x80) != 0;
                e.linear_counter_control = (data & 0x80) != 0;
                e.linear_counter_latch = (data & 0x7f);
                break;

            case 1:
                break;

            case 2:
                e.period = (e.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 3:
                e.period = (e.period & 0x0ff) | ((data << 8) & 0x700);
                e.linear_counter_reload = true;

                if (e.enabled)
                {
                    Duration.write(e.duration, data);
                }
                break;
            }
        }
    }
}
