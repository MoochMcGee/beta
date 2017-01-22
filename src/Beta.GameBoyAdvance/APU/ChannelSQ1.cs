using System;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelSQ1
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

        public ChannelSQ1()
        {
            cycles =
            period = (2048 - frequency) * 16 * Apu.Single;
        }

        public void WriteRegister1(uint address, byte data)
        {
            sweepPeriod = (data >> 4) & 7;
            sweepDelta = 1 - ((data >> 2) & 2);
            sweepShift = (data >> 0) & 7;

            base_WriteRegister1(address, data &= 0x7f);
        }

        public void WriteRegister2(uint address, byte data) { }

        public void WriteRegister3(uint address, byte data)
        {
            form = data >> 6;
            duration.Refresh = (data & 0x3F);
            duration.Counter = 64 - duration.Refresh;

            base_WriteRegister3(address, data &= 0xc0);
        }

        public void WriteRegister4(uint address, byte data)
        {
            envelope.Level = (data >> 4 & 0xF);
            envelope.Delta = (data >> 2 & 0x2) - 1;
            envelope.Period = (data & 0x7);

            base_WriteRegister4(address, data);
        }

        public void WriteRegister5(uint address, byte data)
        {
            frequency = (frequency & 0x700) | (data << 0 & 0x0FF);
            period = (2048 - frequency) * 16 * Apu.Single;
        }

        public void WriteRegister6(uint address, byte data)
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

            base_WriteRegister6(address, data &= 0x40);
        }

        public void WriteRegister7(uint address, byte data) { }

        public void WriteRegister8(uint address, byte data) { }

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











        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int frequency;
        public int cycles;
        public int period;

        public bool lenable;
        public bool renable;

        public byte ReadRegister1(uint address) { return registers[0]; }

        public byte ReadRegister2(uint address) { return registers[1]; }

        public byte ReadRegister3(uint address) { return registers[2]; }

        public byte ReadRegister4(uint address) { return registers[3]; }

        public byte ReadRegister5(uint address) { return registers[4]; }

        public byte ReadRegister6(uint address) { return registers[5]; }

        public byte ReadRegister7(uint address) { return registers[6]; }

        public byte ReadRegister8(uint address) { return registers[7]; }

        public void base_WriteRegister1(uint address, byte data) { registers[0] = data; }

        public void base_WriteRegister2(uint address, byte data) { registers[1] = data; }

        public void base_WriteRegister3(uint address, byte data) { registers[2] = data; }

        public void base_WriteRegister4(uint address, byte data) { registers[3] = data; }

        public void base_WriteRegister5(uint address, byte data) { registers[4] = data; }

        public void base_WriteRegister6(uint address, byte data) { registers[5] = data; }

        public void base_WriteRegister7(uint address, byte data) { registers[6] = data; }

        public void base_WriteRegister8(uint address, byte data) { registers[7] = data; }

        public void ClockDuration()
        {
            if (duration.Clock())
            {
                active = false;
            }
        }
    }
}
