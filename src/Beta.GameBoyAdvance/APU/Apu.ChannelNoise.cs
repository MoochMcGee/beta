using System;
using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu
    {
        private class ChannelNoise : Channel
        {
            private static int[] divisorTable = new[]
            {
                0x08, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70
            };

            private int shift = 8;
            private int value = 0x6000;

            public ChannelNoise(MMIO mmio, Timing timing)
                : base(mmio, timing)
            {
                this.timing.Cycles =
                this.timing.Period = divisorTable[0] * 4 * timing.Single;
                this.timing.Single = timing.Single;
            }

            // 4000078h - NR41 - Channel 4 Duration (R/W)
            //   Bit        Expl.
            //   0-5   -/W  Sound length; units of (64-n)/256s  (0-63)
            //   6-7   -/-  Not used
            //  The Length value is used only if Bit 6 in NR44 is set.

            // 4000079h - NR42 - Channel 4 Envelope (R/W)
            //   Bit        Expl.
            //   0-2   R/W  Envelope Step-Time; units of n/64s  (1-7, 0=No Envelope)
            //     3   R/W  Envelope Direction                  (0=Decrease, 1=Increase)
            //   4-7   R/W  Initial Volume of envelope          (1-15, 0=No Sound)

            // 400007Ah - Not Used
            // 400007Bh - Not Used

            // 400007Ch - SOUND4CNT_H (NR43, NR44) - Channel 4 Frequency/Control (R/W)
            //   Bit        Expl.
            //   0-2   R/W  Dividing Ratio of Frequencies (r)
            //     3   R/W  Counter Step/Width (0=15 bits, 1=7 bits)
            //   4-7   R/W  Shift Clock Frequency (s)
            //  Frequency = 524288 Hz / r / 2^(s+1) ;For r=0 assume r=0.5 instead

            // 400007Dh - Not Used
            //   Bit        Expl.
            //   0-5   -/-  Not used
            //     6   R/W  Length Flag  (1=Stop output when length in NR41 expires)
            //     7   -/W  Initial      (1=Restart Sound)

            // 400007Eh - Not Used
            // 400007Fh - Not Used

            protected override void WriteRegister1(uint address, byte data)
            {
                duration.Refresh = (data & 0x3F);
                duration.Counter = 64 - duration.Refresh;
            }

            protected override void WriteRegister2(uint address, byte data)
            {
                envelope.Level = (data >> 4 & 0xF);
                envelope.Delta = (data >> 2 & 0x2) - 1;
                envelope.Timing.Period = (data & 0x7);

                base.WriteRegister2(address, data);
            }

            protected override void WriteRegister3(uint address, byte data)
            {
            }

            protected override void WriteRegister4(uint address, byte data)
            {
            }

            protected override void WriteRegister5(uint address, byte data)
            {
                shift = data & 0x8;

                timing.Period = (divisorTable[data & 0x7] << (data >> 4)) * 4 * timing.Single;

                base.WriteRegister5(address, data);
            }

            protected override void WriteRegister6(uint address, byte data)
            {
                if (data >= 0x80)
                {
                    active = true;
                    timing.Cycles = timing.Period;

                    duration.Counter = 64 - duration.Refresh;
                    envelope.Timing.Cycles = envelope.Timing.Period;
                    envelope.CanUpdate = true;

                    value = 0x4000 >> shift;
                }

                duration.Enabled = (data & 0x40) != 0;

                if ((registers[1] & 0xF8) == 0)
                {
                    active = false;
                }

                base.WriteRegister6(address, data &= 0x40);
            }

            protected override void WriteRegister7(uint address, byte data)
            {
            }

            protected override void WriteRegister8(uint address, byte data)
            {
            }

            public void ClockEnvelope()
            {
                envelope.Clock();
            }

            public int Render(int cycles)
            {
                var sum = timing.Cycles;
                timing.Cycles -= cycles;

                if (active)
                {
                    if (timing.Cycles >= 0)
                    {
                        if ((value & 0x1) != 0)
                            return (byte)envelope.Level;
                    }
                    else
                    {
                        if ((value & 0x1) == 0)
                            sum = 0;

                        for (; timing.Cycles < 0; timing.Cycles += timing.Period)
                        {
                            //int feedback = (((value >> 1) ^ value) & 1);
                            //value = ((value >> 1) | (feedback << xor));

                            if ((value & 0x1) != 0)
                            {
                                value = (value >> 1) ^ (0x6000 >> shift);
                                sum += Math.Min(-timing.Cycles, timing.Period);
                            }
                            else
                            {
                                value = (value >> 1);
                            }
                        }

                        return (byte)((sum * envelope.Level) / cycles);
                    }
                }
                else
                {
                    for (; timing.Cycles < 0; timing.Cycles += timing.Period)
                    {
                        if ((value & 0x01) != 0)
                            value = (value >> 1) ^ (0x6000 >> shift);
                        else
                            value = (value >> 1);

                        //int feedback = (value ^ (value >> 1)) & 0x1;
                        //value = ((value >> 1) | (feedback << shift));
                    }
                }

                return 0;
            }
        }
    }
}
