namespace Beta.Famicom.CPU
{
    public sealed class NoiStateManager
    {
        private static readonly int[] period_lut = new[]
        {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };

        private readonly NoiState noi;

        public NoiStateManager(State state)
        {
            this.noi = state.r2a03.noi;
        }

        public void Write(ushort address, byte data)
        {
            switch (address - 0x400c)
            {
            case 0:
                noi.duration.halted = (data & 0x20) != 0;
                noi.envelope.looping = (data & 0x20) == 0;
                noi.envelope.constant = (data & 0x10) != 0;
                noi.envelope.period = (data >> 0) & 15;
                break;

            case 1:
                break;

            case 2:
                noi.lfsr_mode = (data >> 7) & 1;
                noi.period = period_lut[data & 15];
                break;

            case 3:
                noi.envelope.start = true;

                if (noi.enabled)
                {
                    noi.duration.counter = Duration.duration_lut[data >> 3];
                }
                break;
            }
        }
    }
}
