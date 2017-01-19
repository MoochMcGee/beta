using System;
using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelSQ1 : Channel
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

        private bool sweepEnable;
        private int sweepCycles;
        private int sweepDelta = 1;
        private int sweepShift;
        private int sweepShadow;
        private int sweepPeriod;

        public override bool Enabled
        {
            get { return active; }
        }

        public ChannelSQ1(MMIO mmio)
            : base(mmio)
        {
            cycles = period = (2048 - frequency) * 16 * Apu.Single;
        }

        protected override void WriteRegister1(uint address, byte data)
        {
            sweepPeriod = (data >> 4 & 0x7);
            sweepDelta = 1 - (data >> 2 & 0x2);
            sweepShift = (data >> 0 & 0x7);

            base.WriteRegister1(address, data &= 0x7f);
        }

        protected override void WriteRegister2(uint address, byte data)
        {
            base.WriteRegister2(address, data &= 0x00);
        }

        protected override void WriteRegister3(uint address, byte data)
        {
            form = data >> 6;
            duration.Refresh = (data & 0x3F);
            duration.Counter = 64 - duration.Refresh;

            base.WriteRegister3(address, data &= 0xc0);
        }

        protected override void WriteRegister4(uint address, byte data)
        {
            envelope.Level = (data >> 4 & 0xF);
            envelope.Delta = (data >> 2 & 0x2) - 1;
            envelope.Period = (data & 0x7);

            base.WriteRegister4(address, data &= 0xff);
        }

        protected override void WriteRegister5(uint address, byte data)
        {
            frequency = (frequency & 0x700) | (data << 0 & 0x0FF);
            period = (2048 - frequency) * 16 * Apu.Single;

            base.WriteRegister5(address, data &= 0x00);
        }

        protected override void WriteRegister6(uint address, byte data)
        {
            frequency = (frequency & 0x0FF) | (data << 8 & 0x700);
            period = (2048 - frequency) * 16 * Apu.Single;

            if ((data & 0x80) != 0)
            {
                active = true;
                cycles = period;

                duration.Counter = 64 - duration.Refresh;
                envelope.Cycles = envelope.Period;
                envelope.CanUpdate = true;

                sweepShadow = frequency;
                sweepCycles = sweepPeriod;
                sweepEnable = (sweepShift != 0 || sweepPeriod != 0);

                step = 7;
            }

            duration.Enabled = (data & 0x40) != 0;

            if ((registers[3] & 0xF8) == 0)
            {
                active = false;
            }

            base.WriteRegister6(address, data &= 0x40);
        }

        protected override void WriteRegister7(uint address, byte data)
        {
            base.WriteRegister7(address, 0);
        }

        protected override void WriteRegister8(uint address, byte data)
        {
            base.WriteRegister8(address, 0);
        }

        public void ClockEnvelope()
        {
            envelope.Clock();
        }

        public void ClockSweep()
        {
            if (!ClockDown() || !sweepEnable || sweepPeriod == 0)
            {
                return;
            }

            var result = sweepShadow + ((sweepShadow >> sweepShift) * sweepDelta);

            if (result > 0x7ff)
            {
                active = false;
            }
            else if (sweepShift != 0)
            {
                sweepShadow = result;
                period = (2048 - sweepShadow) * 16 * Apu.Single;
            }
        }

        public int Render(int t)
        {
            var sum = cycles;
            cycles -= t;

            if (active)
            {
                if (cycles >= 0)
                {
                    return (byte)(envelope.Level >> dutyTable[form][step]);
                }

                sum >>= dutyTable[form][step];

                for (; cycles < 0; cycles += period)
                {
                    sum += Math.Min(-cycles, period) >> dutyTable[form][step = (step - 1) & 0x7];
                }

                return (byte)((sum * envelope.Level) / t);
            }

            for (; cycles < 0; cycles += period)
            {
                step = (step - 1) & 0x7;
            }

            return 0;
        }


        public bool ClockDown()
        {
            sweepCycles--;

            if (sweepCycles <= 0)
            {
                sweepCycles += sweepPeriod;
                return true;
            }

            return false;
        }
    }
}
