using System;

namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        //       Noise
        //     FF1F ---- ---- Not used
        //NR41 FF20 --LL LLLL Length load (64-L)
        //NR42 FF21 VVVV APPP Starting volume, Envelope add mode, period
        //NR43 FF22 SSSS WDDD Clock shift, Width mode of LFSR, Divisor code
        //NR44 FF23 TL-- ---- Trigger, Length enable

        private sealed class ChannelNoi : Channel
        {
            private static int[] divisorTable = new[]
            {
                0x08, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70
            };

            private int shift = 14;
            private int value = 0x0001;

            public ChannelNoi(IAddressSpace addressSpace)
                : base(addressSpace, 3)
            {
                Timing.Cycles =
                Timing.Single = divisorTable[0] * PHASE / 4;
                Timing.Period = DELAY;
            }

            protected override void OnWriteReg2(byte data)
            {
                Duration.Refresh = (data & 0x3F);
                Duration.Counter = 64 - Duration.Refresh;
            }

            protected override void OnWriteReg3(byte data)
            {
                Envelope.Level = (data >> 4 & 0xF);
                Envelope.Delta = (data >> 2 & 0x2) - 1;
                Envelope.Timing.Period = (data & 0x7);

                if ((Registers[2] & 0xF8) == 0)
                    Active = false;
            }

            protected override void OnWriteReg4(byte data)
            {
                shift = 14 - (data & 0x08);
                Timing.Single = (divisorTable[data & 0x7] << (data >> 4)) * PHASE / 4;
            }

            protected override void OnWriteReg5(byte data)
            {
                if ((data & 0x80) != 0)
                {
                    Active = true;
                    Timing.Cycles = Timing.Single;

                    if (Duration.Counter == 0)
                        Duration.Counter = 64;

                    Envelope.Timing.Cycles = Envelope.Timing.Period;
                    Envelope.CanUpdate = true;

                    value = 0x7FFF;
                }

                Duration.Enabled = (data & 0x40) != 0;

                if ((Registers[2] & 0xF8) == 0)
                    Active = false;
            }

            public byte Sample()
            {
                var sum = Timing.Cycles;
                Timing.Cycles -= Timing.Period;

                if ((Registers[2] & 0xF8) != 0 && Active)
                {
                    if (Timing.Cycles >= 0)
                    {
                        if ((value & 0x1) == 0)
                            return (byte)Envelope.Level;
                    }
                    else
                    {
                        if ((value & 0x1) != 0)
                            sum = 0;

                        for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                        {
                            var feedback = (value ^ (value >> 1)) & 0x1;
                            value = ((value >> 1) | (feedback << shift));

                            if ((value & 0x1) == 0)
                                sum += Math.Min(-Timing.Cycles, Timing.Single);
                        }

                        return (byte)((sum * Envelope.Level) / Timing.Period);
                    }
                }
                else
                {
                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        var feedback = (value ^ (value >> 1)) & 0x1;
                        value = ((value >> 1) | (feedback << shift));
                    }
                }

                return 0;
            }

            public void ClockEnvelope()
            {
                Envelope.Clock();
            }
        }
    }
}
