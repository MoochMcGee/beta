using System;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelSQ2
    {
        private static byte[][] dutyTable = new[]
        {
            new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
            new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
            new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x00, 0x00, 0x00 },
            new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f }
        };

        private readonly Sq2State state;

        public ChannelSQ2(ApuState state)
        {
            this.state = state.sound_2;
            this.state.cycles =
            this.state.period = FrequencyToPeriod(0);
        }

        public byte ReadReg(uint address) => state.registers[address & 7];

        public void Write068(uint address, byte data)
        {
            state.duty_form = data >> 6;
            state.duration.Write1(data);

            state.registers[0] = data &= 0xc0;
        }

        public void Write069(uint address, byte data)
        {
            state.envelope.Write(data);

            state.registers[1] = data;
        }

        public void Write06A(uint address, byte data) { }

        public void Write06B(uint address, byte data) { }

        public void Write06C(uint address, byte data)
        {
            state.frequency = (state.frequency & 0x700) | ((data << 0) & 0x0FF);
            state.period = FrequencyToPeriod(state.frequency);
        }

        public void Write06D(uint address, byte data)
        {
            state.frequency = (state.frequency & 0x0FF) | ((data << 8) & 0x700);
            state.period = FrequencyToPeriod(state.frequency);

            if ((data & 0x80) != 0)
            {
                state.active = true;
                state.cycles = state.period;

                state.duration.Reset();
                state.envelope.Reset();

                state.duty_step = 7;
            }

            state.duration.Write2(data);

            if ((state.registers[1] & 0xF8) == 0)
            {
                state.active = false;
            }

            state.registers[5] = data &= 0x40;
        }

        public void Write06E(uint address, byte data) { }

        public void Write06F(uint address, byte data) { }

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
