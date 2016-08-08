namespace Beta.Famicom.CPU
{
    public sealed class Sq1Registers
    {
        private readonly Sq1State sq1;

        public Sq1Registers(State state)
        {
            this.sq1 = state.r2a03.sq1;
        }

        public void Write(ushort address, byte data)
        {
            switch (address - 0x4000)
            {
            case 0:
                sq1.duty_form = (data >> 6) & 3;
                sq1.duration.halted = (data & 0x20) != 0;
                sq1.envelope.looping = (data & 0x20) == 0;
                sq1.envelope.constant = (data & 0x10) != 0;
                sq1.envelope.period = (data >> 0) & 15;
                break;

            case 1:
                sq1.sweep.enabled = (data & 0x80) != 0;
                sq1.sweep.period = (data >> 4) & 7;
                sq1.sweep.negated = (data & 0x08) != 0;
                sq1.sweep.shift = (data >> 0) & 7;
                sq1.sweep.reload = true;
                break;

            case 2:
                sq1.period = (sq1.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 3:
                sq1.period = (sq1.period & 0x0ff) | ((data << 8) & 0x700);
                sq1.duty_step = 0;
                sq1.envelope.start = true;

                if (sq1.enabled)
                {
                    sq1.duration.counter = Duration.duration_lut[data >> 3];
                }
                break;
            }
        }
    }
}
