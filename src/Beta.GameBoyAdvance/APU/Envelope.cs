using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class Envelope
    {
        public Timing Timing;
        public bool CanUpdate = true;
        public int Delta;
        public int Level;

        public Envelope()
        {
            Timing.Single = 1;
        }

        public void Clock()
        {
            if (Timing.Period != 0 && Timing.Cycles != 0 && Timing.ClockDown() && CanUpdate)
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
    }
}
