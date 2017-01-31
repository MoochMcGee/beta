using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public sealed class Sq1Registers
    {
        private readonly Sq1State sq1;
        private readonly byte[] regs = new byte[5];

        public Sq1Registers(State state)
        {
            this.sq1 = state.apu.sq1;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff10: return (byte)(0x80 | regs[0]);
            case 0xff11: return (byte)(0x3f | regs[1]);
            case 0xff12: return (byte)(0x00 | regs[2]);
            case 0xff13: return (byte)(0xff | regs[3]);
            case 0xff14: return (byte)(0xbf | regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public void Write(ushort address, byte data)
        {
            regs[address - 0xff10] = data;

            switch (address)
            {
            case 0xff10:
                sq1.sweep_period = (data >> 4) & 7;
                sq1.sweep_direction = (data >> 3) & 1;
                sq1.sweep_shift = (data >> 0) & 7;
                break;

            case 0xff11:
                sq1.duty_form = (data >> 6) & 3;
                sq1.duration.counter = 64 - (data & 63);
                break;

            case 0xff12:
                sq1.dac_power = (data & 0xf8) != 0;
                if (!sq1.dac_power)
                {
                    sq1.enabled = false;
                }

                sq1.envelope.latch = (data >> 4) & 15;
                sq1.envelope.direction = (data >> 3) & 1;
                sq1.envelope.period = (data >> 0) & 7;
                break;

            case 0xff13:
                sq1.period = (sq1.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff14:
                sq1.period = (sq1.period & 0x0ff) | ((data << 8) & 0x700);
                sq1.duration.enabled = (data & 0x40) != 0;

                if ((data & 0x80) != 0 && sq1.dac_power)
                {
                    sq1.timer = (0x800 - sq1.period) * 4;
                    sq1.envelope.counter = sq1.envelope.latch;
                    sq1.envelope.timer = sq1.envelope.period;
                    sq1.enabled = true;

                    if (sq1.duration.counter == 0)
                    {
                        sq1.duration.counter = 64;
                    }

                    sq1.sweep_timer = sq1.sweep_period;
                    sq1.sweep_enabled = sq1.sweep_period != 0 || sq1.sweep_shift != 0;
                }
                break;
            }
        }
    }
}
