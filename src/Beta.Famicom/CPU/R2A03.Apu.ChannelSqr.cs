using System;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private sealed class ChannelSqr : Channel
        {
            private static byte[][] dutyTable = new[]
            {
                new byte[] { 0x1f, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f },
                new byte[] { 0x1f, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f },
                new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f },
                new byte[] { 0x00, 0x1f, 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00 }
            };

            private bool sweepReload;
            private bool validFrequency;
            private int form;
            private int step;
            private int sweepTimer;
            private int sweepIncrease;
            private int sweepDelay;
            private int sweepShift;

            public Duration Duration = new Duration();
            public Envelope Envelope = new Envelope();

            public bool Enabled
            {
                get { return Duration.Counter != 0; }
                set { Duration.SetEnabled(value); }
            }

            public ChannelSqr()
            {
                Timing.Cycles =
                Timing.Single = (Frequency + 1) * PHASE * 2;
            }

            private void UpdateFrequency()
            {
                if ((Frequency >= 0x8) && ((Frequency + (sweepIncrease & (Frequency >> sweepShift))) <= 0x7ff))
                {
                    Timing.Single = (Frequency + 1) * PHASE * 2;
                    validFrequency = true;
                }
                else
                {
                    validFrequency = false;
                }
            }

            public void ClockSweep(int complement)
            {
                if (sweepDelay != 0 && --sweepTimer == 0)
                {
                    sweepTimer = sweepDelay;

                    if (Frequency >= 8)
                    {
                        var num = Frequency >> sweepShift;

                        if (sweepIncrease == 0)
                        {
                            Frequency -= num - complement;
                            UpdateFrequency();
                        }
                        else if ((Frequency + num) <= 0x7ff)
                        {
                            Frequency += num;
                            UpdateFrequency();
                        }
                    }
                }

                if (sweepReload)
                {
                    sweepReload = false;
                    sweepTimer = sweepDelay;
                }
            }

            public void PokeReg1(ushort address, ref byte data)
            {
                form = (data >> 6);
                Envelope.Write(data);
                Duration.Halted = (data & 0x20) != 0;
            }

            public void PokeReg2(ushort address, ref byte data)
            {
                sweepIncrease = ((data & 0x08) != 0) ? 0 : ~0;
                sweepShift = data & 0x07;
                sweepDelay = 0;

                if ((data & 0x87) > 0x80)
                {
                    sweepDelay = ((data >> 4) & 7) + 1;
                    sweepReload = true;
                }

                UpdateFrequency();
            }

            public void PokeReg3(ushort address, ref byte data)
            {
                Frequency = (Frequency & 0x700) | ((data << 0) & 0x0ff);

                UpdateFrequency();
            }

            public void PokeReg4(ushort address, ref byte data)
            {
                Frequency = (Frequency & 0x0ff) | ((data << 8) & 0x700);

                Duration.SetCounter(data);
                Envelope.Reset = true;
                step = 0;

                UpdateFrequency();
            }

            public byte Render()
            {
                var sum = Timing.Cycles;
                Timing.Cycles -= DELAY;

                if (Duration.Counter != 0 && Envelope.Level != 0 && validFrequency)
                {
                    if (Timing.Cycles >= 0)
                    {
                        return (byte)(Envelope.Level >> dutyTable[form][step]);
                    }

                    sum >>= dutyTable[form][step];

                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        sum += Math.Min(-Timing.Cycles, Timing.Single) >> dutyTable[form][step = (step - 1) & 0x7];
                    }

                    return (byte)((sum * Envelope.Level) / DELAY);
                }

                var count = (~Timing.Cycles + Timing.Single) / Timing.Single;

                step = (step - count) & 0x7;
                Timing.Cycles += (count * Timing.Single);

                return 0;
            }
        }
    }
}
