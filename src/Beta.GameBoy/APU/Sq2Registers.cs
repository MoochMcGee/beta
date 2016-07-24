using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public sealed class Sq2Registers
    {
        private readonly Sq2State sq2;
        private readonly byte[] regs = new byte[5];

        public Sq2Registers(State state)
        {
            this.sq2 = state.apu.sq2;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff15: return (byte)(0xff | regs[0]);
            case 0xff16: return (byte)(0x3f | regs[1]);
            case 0xff17: return (byte)(0x00 | regs[2]);
            case 0xff18: return (byte)(0xff | regs[3]);
            case 0xff19: return (byte)(0xbf | regs[4]);
            }

            throw new CompilerPleasingException();
        }

        public void Write(ushort address, byte data)
        {
            regs[address - 0xff15] = data;

            switch (address)
            {
            case 0xff15: break;
            case 0xff16:
                sq2.duty_form = (data >> 6) & 3;
                sq2.duration.latch = (data >> 0) & 63;
                sq2.duration.count = 64 - sq2.duration.latch;
                break;

            case 0xff17:
                sq2.dac_power = (data & 0xf8) != 0;
                if (!sq2.dac_power)
                {
                    sq2.enabled = false;
                }

                sq2.envelope.latch = (data >> 4) & 15;
                sq2.envelope.direction = (data >> 3) & 1;
                sq2.envelope.period = (data >> 0) & 7;
                break;

            case 0xff18:
                sq2.period = (sq2.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff19:
                sq2.period = (sq2.period & 0x0ff) | ((data << 8) & 0x700);
                sq2.duration.loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0 && sq2.dac_power)
                {
                    sq2.timer = (0x800 - sq2.period) * 4;
                    sq2.envelope.count = sq2.envelope.latch;
                    sq2.envelope.timer = sq2.envelope.period;
                    sq2.enabled = true;

                    if (sq2.duration.count == 0)
                    {
                        sq2.duration.count = 64;
                    }
                }
                break;
            }
        }
    }
}
