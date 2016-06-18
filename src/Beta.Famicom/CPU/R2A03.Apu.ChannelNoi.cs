using System;
using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private sealed class ChannelNoi : Channel
        {
            private static int[][] periodTable = new[]
            {
                new[] { 4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068 },
                new[] { 4, 8, 14, 30, 60, 88, 118, 148, 188, 236, 354, 472, 708,  944, 1890, 3778 }
            };

            private int shift = 13;
            private int value = 0x3fff;

            public Duration Duration = new Duration();
            public Envelope Envelope = new Envelope();

            public bool Enabled
            {
                get { return Duration.Counter != 0; }
                set { Duration.SetEnabled(value); }
            }

            public ChannelNoi()
            {
                Timing = new Timing(236250000, 264);
                Timing.Cycles =
                Timing.Single = periodTable[0][0] * PHASE;
            }

            public void PokeReg1(ushort address, ref byte data)
            {
                Envelope.Write(data);
                Duration.Halted = (data & 0x20) != 0;
            }

            public void PokeReg2(ushort address, ref byte data)
            {
            }

            public void PokeReg3(ushort address, ref byte data)
            {
                shift = (data & 0x80) != 0 ? 8 : 13;
                Timing.Single = periodTable[0][data & 0x0f] * PHASE;
            }

            public void PokeReg4(ushort address, ref byte data)
            {
                Duration.SetCounter(data);
                Envelope.Reset = true;
            }

            public byte Render()
            {
                var sum = Timing.Cycles;
                Timing.Cycles -= DELAY;

                if (Duration.Counter != 0 && Envelope.Level != 0)
                {
                    if (Timing.Cycles >= 0)
                    {
                        if ((value & 0x0001) == 0)
                        {
                            return Envelope.Level;
                        }
                    }
                    else
                    {
                        if ((value & 0x0001) != 0)
                        {
                            sum = 0;
                        }

                        for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                        {
                            value = ((value << 14 ^ value << shift) & 0x4000) | (value >> 1);

                            if ((value & 0x0001) == 0)
                            {
                                sum += Math.Min(-Timing.Cycles, Timing.Single);
                            }
                        }

                        return (byte)((sum * Envelope.Level) / DELAY);
                    }
                }
                else
                {
                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        value = ((value << 14 ^ value << shift) & 0x4000) | (value >> 1);
                    }
                }

                return 0;
            }
        }
    }
}
