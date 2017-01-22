namespace Beta.GameBoyAdvance.APU
{
    public sealed class Envelope
    {
        public bool CanUpdate = true;
        public int Delta;
        public int Level;
        public int Cycles;
        public int Period;

        public void Clock()
        {
            if (Period != 0 && Cycles != 0 && ClockDown() && CanUpdate)
            {
                var value = (Level + Delta) & 0xFF;

                if (value < 0x10)
                {
                    Level = value;
                }
                else
                {
                    CanUpdate = false;
                }
            }
        }

        public bool ClockDown()
        {
            Cycles--;

            if (Cycles <= 0)
            {
                Cycles += Period;
                return true;
            }

            return false;
        }
    }
}
