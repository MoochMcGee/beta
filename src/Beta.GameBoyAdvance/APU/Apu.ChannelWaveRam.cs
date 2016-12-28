using System;
using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu
    {
        private class ChannelWaveRam : Channel
        {
            private static int[] volumeTable = new[]
            {
                4, 0, 1, 2
            };

            private byte[][] amp = Utility.CreateArray<byte>(2, 32);
            private byte[][] ram = Utility.CreateArray<byte>(2, 16);
            private int bank;
            private int count;
            private int dimension;
            private int shift = volumeTable[0];

            public ChannelWaveRam(MMIO mmio, Timing timing)
                : base(mmio, timing)
            {
                this.timing.Cycles =
                this.timing.Period = (2048 - frequency) * 8 * timing.Single;
                this.timing.Single = timing.Single;
            }

            public byte Read(uint address)
            {
                return ram[bank ^ 1][address & 0x0F];
            }

            public void Write(uint address, byte data)
            {
                ram[bank ^ 1][address & 0x0F] = data;

                address = (address << 1) & 0x1E;

                amp[bank ^ 1][address | 0x00] = (byte)(data >> 4 & 0xF);
                amp[bank ^ 1][address | 0x01] = (byte)(data >> 0 & 0xF);
            }

            protected override void WriteRegister1(uint address, byte data)
            {
                dimension = (data >> 5) & 1;
                bank = (data >> 6) & 1;

                if ((data & 0x80) == 0)
                {
                    active = false;
                }

                base.WriteRegister1(address, data &= 0xe0);
            }

            protected override void WriteRegister2(uint address, byte data)
            {
                base.WriteRegister2(address, 0);
            }

            protected override void WriteRegister3(uint address, byte data)
            {
                duration.Refresh = data;
                duration.Counter = 256 - duration.Refresh;

                base.WriteRegister3(address, 0);
            }

            protected override void WriteRegister4(uint address, byte data)
            {
                shift = volumeTable[data >> 5 & 0x3];

                base.WriteRegister4(address, data &= 0xe0);
            }

            protected override void WriteRegister5(uint address, byte data)
            {
                frequency = (frequency & ~0x0FF) | (data << 0 & 0x0FF);
                timing.Period = (2048 - frequency) * 8 * timing.Single;

                base.WriteRegister5(address, 0);
            }

            protected override void WriteRegister6(uint address, byte data)
            {
                frequency = (frequency & ~0x700) | (data << 8 & 0x700);
                timing.Period = (2048 - frequency) * 8 * timing.Single;

                if ((data & 0x80) != 0)
                {
                    active = true;
                    timing.Cycles = timing.Single;

                    duration.Counter = 256 - duration.Refresh;

                    count = 0;
                }

                duration.Enabled = (data & 0x40) != 0;

                base.WriteRegister6(address, data &= 0x40);
            }

            protected override void WriteRegister7(uint address, byte data)
            {
                base.WriteRegister7(address, 0);
            }

            protected override void WriteRegister8(uint address, byte data)
            {
                base.WriteRegister8(address, 0);
            }

            public int Render(int cycles)
            {
                if ((registers[0] & 0x80) != 0)
                {
                    var sum = timing.Cycles;
                    timing.Cycles -= cycles;

                    if (active)
                    {
                        if (timing.Cycles < 0)
                        {
                            sum *= amp[bank][count] >> shift;

                            for (; timing.Cycles < 0; timing.Cycles += timing.Period)
                            {
                                count = (count + 1) & 0x1F;

                                if (count == 0)
                                    bank ^= dimension;

                                sum += Math.Min(-timing.Cycles, timing.Period) * amp[bank][count] >> shift;
                            }

                            return (byte)(sum / cycles);
                        }
                    }
                    else if (timing.Cycles < 0)
                    {
                        var c = (~timing.Cycles + timing.Single) / timing.Single;
                        timing.Cycles += (c * timing.Single);
                    }
                }

                return (byte)(amp[bank][count] >> shift);
            }
        }
    }
}
