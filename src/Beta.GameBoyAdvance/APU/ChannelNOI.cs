using System;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelNOI
    {
        private static readonly int[] divisorTable = new[]
        {
            0x08, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70
        };

        private readonly NoiState state;

        public ChannelNOI(ApuState state)
        {
            this.state = state.sound_4;
            this.state.cycles =
            this.state.period = divisorTable[0] * 4 * Apu.Single;
        }

        public byte ReadReg(uint address) => state.registers[address & 7];

        public void Write078(uint address, byte data)
        {
            state.duration.Write1(data);
        }

        public void Write079(uint address, byte data)
        {
            state.envelope.Write(data);

            state.registers[1] = data;
        }

        public void Write07A(uint address, byte data) { }

        public void Write07B(uint address, byte data) { }

        public void Write07C(uint address, byte data)
        {
            state.shift = data & 0x8;

            state.period = (divisorTable[data & 0x7] << (data >> 4)) * 4 * Apu.Single;

            state.registers[4] = data;
        }

        public void Write07D(uint address, byte data)
        {
            if (data >= 0x80)
            {
                state.active = true;
                state.cycles = state.period;

                state.duration.Reset();
                state.envelope.Reset();

                state.value = 0x4000 >> state.shift;
            }

            state.duration.Write2(data);

            if ((state.registers[1] & 0xF8) == 0)
            {
                state.active = false;
            }

            state.registers[5] = data &= 0x40;
        }

        public void Write07E(uint address, byte data) { }

        public void Write07F(uint address, byte data) { }

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

        public int Render(int t)
        {
            var sum = state.cycles;
            state.cycles -= t;

            if (state.active)
            {
                if (state.cycles >= 0)
                {
                    if ((state.value & 0x1) != 0)
                    {
                        return (byte)state.envelope.GetOutput();
                    }
                }
                else
                {
                    if ((state.value & 1) == 0)
                        sum = 0;

                    for (; state.cycles < 0; state.cycles += state.period)
                    {
                        //int feedback = (((value >> 1) ^ value) & 1);
                        //value = ((value >> 1) | (feedback << xor));

                        if ((state.value & 1) != 0)
                        {
                            state.value = (state.value >> 1) ^ (0x6000 >> state.shift);
                            sum += Math.Min(-state.cycles, state.period);
                        }
                        else
                        {
                            state.value = (state.value >> 1);
                        }
                    }

                    return (byte)((sum * state.envelope.GetOutput()) / t);
                }
            }
            else
            {
                for (; state.cycles < 0; state.cycles += state.period)
                {
                    state.value = (state.value & 1) != 0
                        ? (state.value >> 1) ^ (0x6000 >> state.shift)
                        : (state.value >> 1);

                    //int feedback = (value ^ (value >> 1)) & 0x1;
                    //value = ((value >> 1) | (feedback << shift));
                }
            }

            return 0;
        }
    }
}
