namespace Beta.GameBoyAdvance.APU
{
    public sealed class Duration
    {
        private readonly int mask;
        private readonly int size;

        private int enabled;
        private int counter;
        private int counter_latch;

        public Duration(int size)
        {
            this.mask = size - 1;
            this.size = size;
        }

        public bool Clock()
        {
            if (enabled == 0)
            {
                return false;
            }

            if (counter != 0)
            {
                counter--;

                if (counter == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            counter = size - counter_latch;
        }

        public void Write1(byte data)
        {
            counter_latch = (data & mask);
        }

        public void Write2(byte data)
        {
            enabled = (data >> 6) & 1;
        }
    }
}
