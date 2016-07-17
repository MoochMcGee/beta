using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public sealed class WavStateManager
    {
        private readonly WavState wav;
        private readonly byte[] regs = new byte[5];

        public WavStateManager(State state)
        {
            this.wav = state.apu.wav;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff1a: return (byte)(0x7f | regs[0]);
            case 0xff1b: return (byte)(0xff | regs[1]);
            case 0xff1c: return (byte)(0x9f | regs[2]);
            case 0xff1d: return (byte)(0xff | regs[3]);
            case 0xff1e: return (byte)(0xbf | regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public void Write(ushort address, byte data)
        {
            regs[address - 0xff1a] = data;

            switch (address)
            {
            case 0xff1a:
                wav.dac_power = (data & 0x80) != 0;
                if (!wav.dac_power)
                {
                    wav.enabled = false;
                }
                break;

            case 0xff1b:
                wav.duration.latch = data;
                wav.duration.count = 256 - wav.duration.latch;
                break;

            case 0xff1c:
                wav.volume_code = (data >> 5) & 3;

                switch (wav.volume_code)
                {
                case 0: wav.volume_shift = 4; break;
                case 1: wav.volume_shift = 0; break;
                case 2: wav.volume_shift = 1; break;
                case 3: wav.volume_shift = 2; break;
                }
                break;

            case 0xff1d:
                wav.period = (wav.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff1e:
                wav.period = (wav.period & 0x0ff) | ((data << 8) & 0x700);
                wav.duration.loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0 && wav.dac_power)
                {
                    wav.timer = (0x800 - wav.period) * 2;
                    wav.wave_ram_cursor = 0;
                    wav.wave_ram_shift = 4;
                    wav.enabled = true;

                    if (wav.duration.count == 0)
                    {
                        wav.duration.count = 256;
                    }
                }
                break;
            }
        }
    }
}
