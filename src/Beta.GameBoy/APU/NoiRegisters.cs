using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public sealed class NoiRegisters
    {
        private static readonly int[] divisor_lut = new[]
        {
            8, 16, 32, 48, 64, 80, 96, 112
        };

        private readonly NoiState noi;
        private readonly byte[] regs = new byte[5];

        public NoiRegisters(State state)
        {
            this.noi = state.apu.noi;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff1f: return (byte)(0xff | regs[0]);
            case 0xff20: return (byte)(0xff | regs[1]);
            case 0xff21: return (byte)(0x00 | regs[2]);
            case 0xff22: return (byte)(0x00 | regs[3]);
            case 0xff23: return (byte)(0xbf | regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public void Write(ushort address, byte data)
        {
            regs[address - 0xff1f] = data;

            switch (address)
            {
            case 0xff1f: break;
            case 0xff20:
                noi.duration.counter = 64 - (data & 63);
                break;

            case 0xff21:
                noi.dac_power = (data & 0xf8) != 0;
                if (!noi.dac_power)
                {
                    noi.enabled = false;
                }

                noi.envelope.latch = (data >> 4) & 15;
                noi.envelope.direction = (data >> 3) & 1;
                noi.envelope.period = (data >> 0) & 7;
                break;

            case 0xff22:
                noi.lfsr_frequency = (data >> 4) & 15;
                noi.lfsr_mode = (data >> 3) & 1;
                noi.lfsr_divisor = (data >> 0) & 7;

                noi.period = divisor_lut[noi.lfsr_divisor] << noi.lfsr_frequency;
                break;

            case 0xff23:
                noi.duration.enabled = (data & 0x40) != 0;

                if ((data & 0x80) != 0 && noi.dac_power)
                {
                    noi.timer = noi.period;
                    noi.envelope.counter = noi.envelope.latch;
                    noi.envelope.timer = noi.envelope.period;
                    noi.lfsr = 0x7fff;
                    noi.enabled = true;

                    if (noi.duration.counter == 0)
                    {
                        noi.duration.counter = 64;
                    }
                }
                break;
            }
        }
    }
}
