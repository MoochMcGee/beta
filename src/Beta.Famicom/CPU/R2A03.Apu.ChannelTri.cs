using System;
using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private sealed class ChannelTri : Channel
        {
            private static byte[] pyramid = new byte[]
            {
                0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf,
                0xf, 0xe, 0xd, 0xc, 0xb, 0xa, 0x9, 0x8, 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0
            };

            private bool control;
            private bool halt;
            private int counter;
            private int refresh;
            private int step;

            public Duration Duration = new Duration();

            public bool Enabled
            {
                get { return Duration.Counter != 0; }
                set { Duration.SetEnabled(value); }
            }

            public ChannelTri()
            {
                Timing = new Timing(236250000, 264);
                Timing.Cycles =
                Timing.Single = (Frequency + 1) * PHASE;
            }

            public void ClockLinearCounter()
            {
                if (halt)
                {
                    counter = refresh;
                }
                else
                {
                    if (counter != 0)
                    {
                        counter--;
                    }
                }

                halt &= control;
            }

            public void PokeReg1(ushort address, ref byte data)
            {
                control = (data & 0x80) != 0;
                refresh = (data & 0x7f);

                Duration.Halted = (data & 0x80) != 0;
            }

            public void PokeReg2(ushort address, ref byte data)
            {
            }

            public void PokeReg3(ushort address, ref byte data)
            {
                Frequency = (Frequency & 0x700) | ((data << 0) & 0x0ff);
                Timing.Single = (Frequency + 1) * PHASE;
            }

            public void PokeReg4(ushort address, ref byte data)
            {
                Frequency = (Frequency & 0x0ff) | ((data << 8) & 0x700);
                Timing.Single = (Frequency + 1) * PHASE;

                Duration.SetCounter(data);
                halt = true;
            }

            public byte Render()
            {
                var sum = Timing.Cycles;
                Timing.Cycles -= DELAY;

                if (Duration.Counter != 0 && counter != 0 && Frequency > 2 && Timing.Cycles < 0)
                {
                    sum *= pyramid[step];

                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        sum += Math.Min(-Timing.Cycles, Timing.Single) * pyramid[step = (step + 1) & 0x1f];
                    }

                    return (byte)(sum / DELAY);
                }

                Timing.Cycles += ((~Timing.Cycles + Timing.Single) / Timing.Single) * Timing.Single;

                return pyramid[step];
            }
        }
    }
}
