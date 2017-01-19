using System;
using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelWAV : Channel
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

        public ChannelWAV(MMIO mmio)
            : base(mmio)
        {
            cycles =
            period = (2048 - frequency) * 8 * Apu.Single;
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
            period = (2048 - frequency) * 8 * Apu.Single;

            base.WriteRegister5(address, 0);
        }

        protected override void WriteRegister6(uint address, byte data)
        {
            frequency = (frequency & ~0x700) | (data << 8 & 0x700);
            period = (2048 - frequency) * 8 * Apu.Single;

            if ((data & 0x80) != 0)
            {
                active = true;
                cycles = period;

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

        public int Render(int t)
        {
            if ((registers[0] & 0x80) != 0)
            {
                var sum = cycles;
                cycles -= t;

                if (active)
                {
                    if (cycles < 0)
                    {
                        sum *= amp[bank][count] >> shift;

                        for (; cycles < 0; cycles += period)
                        {
                            count = (count + 1) & 0x1F;

                            if (count == 0)
                                bank ^= dimension;

                            sum += Math.Min(-cycles, period) * amp[bank][count] >> shift;
                        }

                        return (byte)(sum / t);
                    }
                }
                else if (cycles < 0)
                {
                    var c = (~cycles + Apu.Single) / Apu.Single;
                    cycles += (c * Apu.Single);
                }
            }

            return (byte)(amp[bank][count] >> shift);
        }
    }
}
