namespace Beta.Famicom.APU
{
    public static class Noi
    {
        static readonly int[] period_lut =
        {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };

        public static int getOutput(NoiState e)
        {
            return e.duration.counter != 0 && (e.lfsr & 1) == 0
                ? Envelope.volume(e.envelope)
                : 0
                ;
        }

        public static void tick(NoiState e)
        {
            e.timer--;

            if (e.timer == 0)
            {
                e.timer = e.period + 1;

                var tap0 = e.lfsr;
                var tap1 = e.lfsr_mode == 1
                    ? e.lfsr >> 6
                    : e.lfsr >> 1
                    ;

                var feedback = (tap0 ^ tap1) & 1;

                e.lfsr = (e.lfsr >> 1) | (feedback << 14);
            }
        }

        public static void write(NoiState e, int address, byte data)
        {
            switch (address - 0x400c)
            {
            case 0:
                e.duration.halted = (data & 0x20) != 0;
                e.envelope.looping = (data & 0x20) == 0;
                e.envelope.constant = (data & 0x10) != 0;
                e.envelope.period = (data >> 0) & 15;
                break;

            case 1:
                break;

            case 2:
                e.lfsr_mode = (data >> 7) & 1;
                e.period = period_lut[data & 15];
                break;

            case 3:
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
