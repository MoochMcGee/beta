namespace Beta.Platform
{
    public struct Timing
    {
        public int Cycles;
        public int Period;
        public int Single;

        public Timing(int period, int single)
        {
            Cycles = 0;
            Period = period;
            Single = single;
        }

        public bool ClockDown()
        {
            Cycles -= Single;

            if (Cycles <= 0)
            {
                Cycles += Period;
                return true;
            }

            return false;
        }
    }
}
