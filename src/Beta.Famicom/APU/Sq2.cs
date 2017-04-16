namespace Beta.Famicom.APU
{
    public static class Sq2
    {
        public static int getOutput(Sq2State e)
        {
            if (e.period < 8 || (e.sweep.target & 0x800) != 0)
            {
                return 0;
            }

            switch (e.duty_form)
            {
            case 0: if (( e.duty_step & 7) < 7) return 0; break;
            case 1: if (( e.duty_step & 7) < 6) return 0; break;
            case 2: if (( e.duty_step & 7) < 4) return 0; break;
            case 3: if ((~e.duty_step & 7) < 6) return 0; break;
            }

            return Envelope.volume(e.envelope);
        }

        public static void tick(Sq2State e)
        {
            e.timer--;

            if (e.timer == 0)
            {
                e.timer = (e.period + 1) * 2;
                e.duty_step = (e.duty_step - 1) & 7;
            }
        }

        public static void write(Sq2State e, int address, byte data)
        {
            switch (address - 0x4004)
            {
            case 0:
                e.duty_form = (data >> 6) & 3;
                e.duration.halted = (data & 0x20) != 0;
                e.envelope.looping = (data & 0x20) == 0;
                e.envelope.constant = (data & 0x10) != 0;
                e.envelope.period = (data >> 0) & 15;
                break;

            case 1:
                e.sweep.enabled = (data & 0x80) != 0;
                e.sweep.period = (data >> 4) & 7;
                e.sweep.negated = (data & 0x08) != 0;
                e.sweep.shift = (data >> 0) & 7;
                e.sweep.reload = true;
                break;

            case 2:
                e.period = (e.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 3:
                e.period = (e.period & 0x0ff) | ((data << 8) & 0x700);
                e.duty_step = 0;
                e.envelope.start = true;

                if (e.enabled)
                {
                    Duration.write(e.duration, data);
                }
                break;
            }
        }
    }
}
