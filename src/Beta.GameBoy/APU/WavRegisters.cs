using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public static class WavRegisters
    {
        public static byte Read(WavState e, ushort address)
        {
            switch (address)
            {
            case 0xff1a: return (byte)(0x7f | e.regs[0]);
            case 0xff1b: return (byte)(0xff | e.regs[1]);
            case 0xff1c: return (byte)(0x9f | e.regs[2]);
            case 0xff1d: return (byte)(0xff | e.regs[3]);
            case 0xff1e: return (byte)(0xbf | e.regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public static void Write(WavState e, ushort address, byte data)
        {
            e.regs[address - 0xff1a] = data;

            switch (address)
            {
            case 0xff1a:
                e.dac_power = (data & 0x80) != 0;
                if (!e.dac_power)
                {
                    e.enabled = false;
                }
                break;

            case 0xff1b:
                e.duration.counter = 256 - data;
                break;

            case 0xff1c:
                e.volume_code = (data >> 5) & 3;

                switch (e.volume_code)
                {
                case 0: e.volume_shift = 4; break;
                case 1: e.volume_shift = 0; break;
                case 2: e.volume_shift = 1; break;
                case 3: e.volume_shift = 2; break;
                }
                break;

            case 0xff1d:
                e.period = (e.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff1e:
                e.period = (e.period & 0x0ff) | ((data << 8) & 0x700);
                e.duration.enabled = (data & 0x40) != 0;

                if ((data & 0x80) != 0 && e.dac_power)
                {
                    e.timer = (0x800 - e.period) * 2;
                    e.wave_ram_cursor = 0;
                    e.wave_ram_shift = 4;
                    e.enabled = true;

                    if (e.duration.counter == 0)
                    {
                        e.duration.counter = 256;
                    }
                }
                break;
            }
        }
    }
}
