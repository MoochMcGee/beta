using System;

namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        //        Wave
        // NR30 FF1A E--- ---- DAC power
        // NR31 FF1B LLLL LLLL Length load (256-L)
        // NR32 FF1C -VV- ---- Volume code (00=0%, 01=100%, 10=50%, 11=25%)
        // NR33 FF1D FFFF FFFF Frequency LSB
        // NR34 FF1E TL-- -FFF Trigger, Length enable, Frequency MSB

        private sealed class ChannelWav : Channel
        {
            private static int[] volumeTable = new[]
            {
                4, 0, 1, 2
            };

            private byte[] amp = new byte[32];
            private byte[] ram = new byte[16];
            private int count;
            private int shift = volumeTable[0];

            public ChannelWav(GameSystem gameboy)
                : base(gameboy, 2)
            {
                Timing.Cycles =
                Timing.Single = PHASE * 2048;
                Timing.Period = DELAY;
            }

            protected override void OnPokeReg1(byte data)
            {
                if ((data & 0x80) == 0)
                {
                    Active = false;
                }
            }

            protected override void OnPokeReg2(byte data)
            {
                Duration.Refresh = data;
                Duration.Counter = 256 - Duration.Refresh;
            }

            protected override void OnPokeReg3(byte data)
            {
                shift = volumeTable[data >> 5 & 0x3];
            }

            protected override void OnPokeReg4(byte data)
            {
                Frequency = (Frequency & ~0x0FF) | (data << 0 & 0x0FF);
                Timing.Single = (2048 - Frequency) * PHASE / 2;
            }

            protected override void OnPokeReg5(byte data)
            {
                Frequency = (Frequency & ~0x700) | (data << 8 & 0x700);
                Timing.Single = (2048 - Frequency) * PHASE / 2;

                if ((data & 0x80) != 0)
                {
                    Active = true;
                    Timing.Cycles = Timing.Single;

                    if (Duration.Counter == 0)
                    {
                        Duration.Counter = 256;
                    }

                    count = 0;
                }

                Duration.Enabled = (data & 0x40) != 0;

                if ((Registers[0] & 0xF8) == 0)
                {
                    Active = false;
                }
            }

            public byte Peek(uint address)
            {
                return ram[address & 0x0F];
            }

            public void Poke(uint address, byte data)
            {
                ram[address & 0x0F] = data;

                address = (address << 1) & 0x1E;

                amp[address | 0x00] = (byte)(data >> 4 & 0xF);
                amp[address | 0x01] = (byte)(data >> 0 & 0xF);
            }

            public byte Sample()
            {
                if (Active)
                {
                    var sum = Timing.Cycles;
                    Timing.Cycles -= Timing.Period;

                    if (Active)
                    {
                        if (Timing.Cycles < 0)
                        {
                            sum *= amp[count] >> shift;

                            for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                            {
                                sum += Math.Min(-Timing.Cycles, Timing.Single) * amp[count = (count + 1) & 0x1F] >> shift;
                            }

                            return (byte)(sum / Timing.Period);
                        }
                    }
                    else if (Timing.Cycles < 0)
                    {
                        var c = (~Timing.Cycles + Timing.Single) / Timing.Single;
                        Timing.Cycles += (c * Timing.Single);
                    }
                }

                return (byte)(amp[count] >> shift);
            }
        }
    }
}
