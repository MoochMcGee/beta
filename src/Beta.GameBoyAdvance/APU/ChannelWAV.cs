using System;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelWAV
    {
        private static readonly int[] volumeTable = new[]
        {
            4, 0, 1, 2
        };

        private readonly WavState state;

        public ChannelWAV(ApuState state)
        {
            this.state = state.sound_3;
            this.state.cycles =
            this.state.period = FrequencyToPeriod(0);
        }

        public byte Read(uint address)
        {
            return state.ram[state.bank ^ 1][address & 0x0F];
        }

        public void Write(uint address, byte data)
        {
            state.ram[state.bank ^ 1][address & 0x0F] = data;

            address = (address << 1) & 0x1E;

            state.amp[state.bank ^ 1][address | 0x00] = (byte)(data >> 4 & 0xF);
            state.amp[state.bank ^ 1][address | 0x01] = (byte)(data >> 0 & 0xF);
        }

        public byte ReadReg(uint address) => state.registers[address & 7];

        public void Write070(uint address, byte data)
        {
            state.dimension = (data >> 5) & 1;
            state.bank = (data >> 6) & 1;

            if ((data & 0x80) == 0)
            {
                state.active = false;
            }

            state.registers[0] = data &= 0xe0;
        }

        public void Write071(uint address, byte data) { }

        public void Write072(uint address, byte data)
        {
            state.duration.Write1(data);
        }

        public void Write073(uint address, byte data)
        {
            state.shift = volumeTable[data >> 5 & 0x3];

            state.registers[3] = data &= 0xe0;
        }

        public void Write074(uint address, byte data)
        {
            state.frequency = (state.frequency & ~0x0FF) | ((data << 0) & 0x0ff);
            state.period = FrequencyToPeriod(state.frequency);
        }

        public void Write075(uint address, byte data)
        {
            state.frequency = (state.frequency & ~0x700) | (data << 8 & 0x700);
            state.period = FrequencyToPeriod(state.frequency);

            if ((data & 0x80) != 0)
            {
                state.active = true;
                state.cycles = state.period;

                state.duration.Reset();

                state.count = 0;
            }

            state.duration.Write2(data);

            if ((state.registers[0] & 0x80) == 0)
            {
                state.active = false;
            }

            state.registers[5] = data &= 0x40;
        }

        public void Write076(uint address, byte data) { }

        public void Write077(uint address, byte data) { }

        public void ClockDuration()
        {
            if (state.duration.Clock())
            {
                state.active = false;
            }
        }

        private int FrequencyToPeriod(int f)
        {
            return (2048 - f) * 8 * Apu.Single;
        }

        public int Render(int t)
        {
            if ((state.registers[0] & 0x80) != 0)
            {
                var sum = state.cycles;
                state.cycles -= t;

                if (state.active)
                {
                    if (state.cycles < 0)
                    {
                        sum *= state.amp[state.bank][state.count] >> state.shift;

                        for (; state.cycles < 0; state.cycles += state.period)
                        {
                            state.count = (state.count + 1) & 0x1F;

                            if (state.count == 0)
                            {
                                state.bank ^= state.dimension;
                            }

                            sum += Math.Min(-state.cycles, state.period) * state.amp[state.bank][state.count] >> state.shift;
                        }

                        return (byte)(sum / t);
                    }
                }
                else if (state.cycles < 0)
                {
                    var c = (~state.cycles + Apu.Single) / Apu.Single;
                    state.cycles += (c * Apu.Single);
                }
            }

            return (byte)(state.amp[state.bank][state.count] >> state.shift);
        }
    }
}
