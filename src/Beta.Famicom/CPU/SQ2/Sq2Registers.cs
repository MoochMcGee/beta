namespace Beta.Famicom.CPU
{
    public sealed class Sq2Registers
    {
        private readonly Sq2State sq2;

        public Sq2Registers(State state)
        {
            this.sq2 = state.r2a03.sq2;
        }

        public void Write(ushort address, byte data)
        {
            switch (address - 0x4004)
            {
            case 0:
                sq2.duty_form = (data >> 6) & 3;
                sq2.duration.halted = (data & 0x20) != 0;
                sq2.envelope.looping = (data & 0x20) == 0;
                sq2.envelope.constant = (data & 0x10) != 0;
                sq2.envelope.period = (data >> 0) & 15;
                break;

            case 1:
                sq2.sweep.enabled = (data & 0x80) != 0;
                sq2.sweep.period = (data >> 4) & 7;
                sq2.sweep.negated = (data & 0x08) != 0;
                sq2.sweep.shift = (data >> 0) & 7;
                sq2.sweep.reload = true;
                break;

            case 2:
                sq2.period = (sq2.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 3:
                sq2.period = (sq2.period & 0x0ff) | ((data << 8) & 0x700);
                sq2.duty_step = 0;
                sq2.envelope.start = true;

                if (sq2.enabled)
                {
                    sq2.duration.counter = Duration.duration_lut[data >> 3];
                }
                break;
            }
        }
    }
}
