using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public static class Sq1Registers
    {
        public static byte Read(Sq1State e, ushort address)
        {
            switch (address)
            {
            case 0xff10: return (byte)(0x80 | e.regs[0]);
            case 0xff11: return (byte)(0x3f | e.regs[1]);
            case 0xff12: return (byte)(0x00 | e.regs[2]);
            case 0xff13: return (byte)(0xff | e.regs[3]);
            case 0xff14: return (byte)(0xbf | e.regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public static void Write(Sq1State e, ushort address, byte data)
        {
            e.regs[address - 0xff10] = data;

            switch (address)
            {
            case 0xff10:
                e.sweep_period = (data >> 4) & 7;
                e.sweep_direction = (data >> 3) & 1;
                e.sweep_shift = (data >> 0) & 7;
                break;

            case 0xff11:
                e.duty_form = (data >> 6) & 3;
                e.duration.counter = 64 - (data & 63);
                break;

            case 0xff12:
                e.dac_power = (data & 0xf8) != 0;
                if (!e.dac_power)
                {
                    e.enabled = false;
                }

                e.envelope.latch = (data >> 4) & 15;
                e.envelope.direction = (data >> 3) & 1;
                e.envelope.period = (data >> 0) & 7;
                break;

            case 0xff13:
                e.period = (e.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff14:
                e.period = (e.period & 0x0ff) | ((data << 8) & 0x700);
                e.duration.enabled = (data & 0x40) != 0;

                if ((data & 0x80) != 0 && e.dac_power)
                {
                    e.timer = (0x800 - e.period) * 4;
                    e.envelope.counter = e.envelope.latch;
                    e.envelope.timer = e.envelope.period;
                    e.enabled = true;

                    if (e.duration.counter == 0)
                    {
                        e.duration.counter = 64;
                    }

                    e.sweep_timer = e.sweep_period;
                    e.sweep_enabled = e.sweep_period != 0 || e.sweep_shift != 0;
                }
                break;
            }
        }
    }
}
