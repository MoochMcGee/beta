namespace Beta.Famicom.CPU
{
    public sealed class TriRegisters
    {
        private readonly TriState tri;

        public TriRegisters(State state)
        {
            this.tri = state.r2a03.tri;
        }

        public void Write(int address, byte data)
        {
            switch (address - 0x4008)
            {
            case 0:
                tri.duration.halted = (data & 0x80) != 0;
                tri.linear_counter_control = (data & 0x80) != 0;
                tri.linear_counter_latch = (data & 0x7f);
                break;

            case 1:
                break;

            case 2:
                tri.period = (tri.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 3:
                tri.period = (tri.period & 0x0ff) | ((data << 8) & 0x700);
                tri.linear_counter_reload = true;

                if (tri.enabled)
                {
                    tri.duration.counter = Duration.duration_lut[data >> 3];
                }
                break;
            }
        }
    }
}
