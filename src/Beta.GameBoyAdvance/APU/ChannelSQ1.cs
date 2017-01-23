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

        private readonly Sq1State state;

        public byte[] registers = new byte[8];

        public ChannelSQ1(ApuState state)
        {
            this.state = state.sound_1;
            this.state.cycles =
            this.state.period = FrequencyToPeriod(0);
        }

        public byte ReadReg(uint address) => registers[address & 7];

        public void Write060(uint address, byte data)
        {
            state.sweep_period = (data >> 4) & 7;
            state.sweep_delta = 1 - ((data >> 2) & 2);
            state.sweep_shift = (data >> 0) & 7;

            registers[0] = data &= 0x7f;
        }

        public void Write061(uint address, byte data) { }

        public void Write062(uint address, byte data)
        {
            state.duty_form = data >> 6;
            state.duration.Write1(data);

            registers[2] = data &= 0xc0;
        }

        public void Write063(uint address, byte data)
        {
            state.envelope.Write(data);

            registers[3] = data;
        }

        public void Write064(uint address, byte data)
        {
            state.frequency = (state.frequency & 0x700) | ((data << 0) & 0x0ff);
            state.period = FrequencyToPeriod(state.frequency);
        }

        public void Write065(uint address, byte data)
        {
            state.frequency = (state.frequency & 0x0ff) | ((data << 8) & 0x700);
            state.period = FrequencyToPeriod(state.frequency);

            if ((data & 0x80) != 0)
            {
                state.active = true;
                state.cycles = state.period;

                state.duration.Reset();
                state.envelope.Reset();

                state.sweep_shadow = state.frequency;
                state.sweep_cycles = state.sweep_period;
                state.sweep_enable = state.sweep_period != 0 || state.sweep_shift != 0;

                state.duty_step = 7;
            }

            state.duration.Write2(data);

            if ((registers[3] & 0xF8) == 0)
            {
                state.active = false;
            }

            registers[5] = data &= 0x40;
        }

        public void Write066(uint address, byte data) { }

        public void Write067(uint address, byte data) { }

        public bool ClockDown()
        {
            state.sweep_cycles--;

            if (state.sweep_cycles <= 0)
            {
                state.sweep_cycles += state.sweep_period;
                return true;
            }

            return false;
        }

        public void ClockDuration()
        {
            if (state.duration.Clock())
            {
                state.active = false;
            }
        }

        public void ClockEnvelope()
        {
            state.envelope.Clock();
        }

        public void ClockSweep()
        {
            if (!ClockDown() || !state.sweep_enable || state.sweep_period == 0)
            {
                return;
            }

            var result = state.sweep_shadow + ((state.sweep_shadow >> state.sweep_shift) * state.sweep_delta);

            if (result > 0x7ff)
            {
                state.active = false;
            }
            else if (state.sweep_shift != 0)
            {
                state.sweep_shadow = result;
                state.period = FrequencyToPeriod(state.sweep_shadow);
            }
        }

        private static int FrequencyToPeriod(int f)
        {
            return (2048 - f) * 16 * Apu.Single;
        }

        public int Render(int t)
        {
            var sum = state.cycles;
            state.cycles -= t;

            if (state.active)
            {
                if (state.cycles >= 0)
                {
                    return (byte)(state.envelope.GetOutput() >> dutyTable[state.duty_form][state.duty_step]);
                }

                sum >>= dutyTable[state.duty_form][state.duty_step];

                for (; state.cycles < 0; state.cycles += state.period)
                {
                    state.duty_step = (state.duty_step - 1) & 0x7;
                    sum += Math.Min(-state.cycles, state.period) >> dutyTable[state.duty_form][state.duty_step];
                }

                return (byte)((sum * state.envelope.GetOutput()) / t);
            }

            for (; state.cycles < 0; state.cycles += state.period)
            {
                state.duty_step = (state.duty_step - 1) & 0x7;
            }

            return 0;
        }
    }
}
