using System;
using Beta.Platform;

namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        //        Square 1
        // NR10 FF10 -PPP NSSS Sweep period, negate, shift
        // NR11 FF11 DDLL LLLL Duty, Length load (64-L)
        // NR12 FF12 VVVV APPP Starting volume, Envelope add mode, period
        // NR13 FF13 FFFF FFFF Frequency LSB
        // NR14 FF14 TL-- -FFF Trigger, Length enable, Frequency MSB

        private sealed class ChannelSq1 : Channel
        {
            private static byte[][] dutyTable = new[]
            {
                new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
                new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
                new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x00, 0x00, 0x00 },
                new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f }
            };

            private int form;
            private int step = 7;

            private Timing sweepTiming;
            private bool sweepEnable;
            private int sweepDelta = 1;
            private int sweepShift;
            private int sweepShadow;

            public ChannelSq1(IAddressSpace addressSpace)
                : base(addressSpace, 0)
            {
                Timing.Cycles =
                Timing.Single = PHASE * 2048;
                Timing.Period = DELAY;

                sweepTiming.Single = 1;
            }

            protected override void OnWriteReg1(byte data)
            {
                sweepTiming.Period = (data >> 4 & 0x7);
                sweepDelta = 1 - (data >> 2 & 0x2);
                sweepShift = (data >> 0 & 0x7);
            }

            protected override void OnWriteReg2(byte data)
            {
                form = data >> 6;
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
                Frequency = (Frequency & 0x700) | (data << 0 & 0x0FF);
                Timing.Single = (2048 - Frequency) * PHASE;
            }

            protected override void OnWriteReg5(byte data)
            {
                Frequency = (Frequency & 0x0FF) | (data << 8 & 0x700);
                Timing.Single = (2048 - Frequency) * PHASE;

                if ((data & 0x80) != 0)
                {
                    Active = true;
                    Timing.Cycles = Timing.Single;

                    if (Duration.Counter == 0)
                    {
                        Duration.Counter = 64;
                    }

                    Envelope.Timing.Cycles = Envelope.Timing.Period;
                    Envelope.CanUpdate = true;

                    sweepShadow = Frequency;
                    sweepTiming.Cycles = sweepTiming.Period;
                    sweepEnable = (sweepShift != 0 || sweepTiming.Period != 0);

                    step = 7;
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
                        return (byte)(Envelope.Level >> dutyTable[form][step]);
                    }

                    sum >>= dutyTable[form][step];

                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        sum += Math.Min(-Timing.Cycles, Timing.Single) >> dutyTable[form][step = (step - 1) & 0x7];
                    }

                    return (byte)((sum * Envelope.Level) / Timing.Period);
                }

                var count = (~Timing.Cycles + Timing.Single) / Timing.Single;

                step = (step - count) & 0x7;
                Timing.Cycles += (count * Timing.Single);

                return 0;
            }

            public void ClockEnvelope()
            {
                Envelope.Clock();
            }

            public void ClockSweep()
            {
                if (!sweepTiming.ClockDown() || !sweepEnable || sweepTiming.Period == 0)
                {
                    return;
                }

                var result = sweepShadow + ((sweepShadow >> sweepShift) * sweepDelta);

                if (result > 0x7FF)
                {
                    Active = false;
                }
                else if (sweepShift != 0)
                {
                    sweepShadow = result;
                    Timing.Single = (2048 - sweepShadow) * PHASE;
                }
            }
        }
    }
}
