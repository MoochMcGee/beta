using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private static int[][][] timingTable = new[]
        {
            new[]
            {
                new[] { 3729, 3728, 3729, 3729 },
                new[] { 3729, 3728, 3729, 3729 + 3726 }
            },
            new[]
            {
                new[] { 4157, 4157, 4156, 4157 },
                new[] { 4157, 4157, 4156, 4157 + 4156 }
            }
        };

        private Timing frameTimer;

        private void InitializeSequence()
        {
            frameTimer.Cycles = timingTable[0][mode][0];
            frameTimer.Single = 1;
        }

        private void ClockSequence()
        {
            if (frameTimer.Cycles != 0 && --frameTimer.Cycles == 0)
            {
                switch (step)
                {
                case 0: ClockQuad(); break;
                case 1: ClockQuad(); ClockHalf(); break;
                case 2: ClockQuad(); break;
                case 3:
                    ClockQuad(); ClockHalf();

                    if (mode == 0 && irqEnabled)
                    {
                        irqPending = true;
                        Irq(1);
                    }
                    break;
                }

                step = (step + 1) & 3;
                frameTimer.Cycles += timingTable[0][mode][step];
            }
        }
    }
}
