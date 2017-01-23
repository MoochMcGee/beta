namespace Beta.GameBoyAdvance.APU
{
    public sealed class Envelope
    {
        private int upward;
        private int volume;
        private int cycles;
        private int period;

        public void Clock()
        {
            if (cycles != 0 && period != 0)
            {
                cycles--;

                if (cycles == 0)
                {
                    cycles = period;

                    if (upward == 1 && volume < 0xf) { volume++; }
                    if (upward == 0 && volume > 0x0) { volume--; }
                }
            }
        }

        public void Reset()
        {
            cycles = period;
        }

        public void Write(byte data)
        {
            volume = (data >> 4) & 15;
            upward = (data >> 3) & 1;
            period = (data >> 0) & 7;
        }

        public int GetOutput()
        {
            return volume;
        }
    }
}
