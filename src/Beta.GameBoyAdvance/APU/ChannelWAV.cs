using System;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelWAV
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

        public ChannelWAV()
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

        public void WriteRegister1(uint address, byte data)
        {
            dimension = (data >> 5) & 1;
            bank = (data >> 6) & 1;

            if ((data & 0x80) == 0)
            {
                active = false;
            }

            base_WriteRegister1(address, data &= 0xe0);
        }

        public void WriteRegister2(uint address, byte data) { }

        public void WriteRegister3(uint address, byte data)
        {
            duration.Refresh = data;
            duration.Counter = 256 - duration.Refresh;
        }

        public void WriteRegister4(uint address, byte data)
        {
            shift = volumeTable[data >> 5 & 0x3];

            base_WriteRegister4(address, data &= 0xe0);
        }

        public void WriteRegister5(uint address, byte data)
        {
            frequency = (frequency & ~0x0FF) | (data << 0 & 0x0FF);
            period = (2048 - frequency) * 8 * Apu.Single;
        }

        public void WriteRegister6(uint address, byte data)
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

            base_WriteRegister6(address, data &= 0x40);
        }

        public void WriteRegister7(uint address, byte data) { }

        public void WriteRegister8(uint address, byte data) { }

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









        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int frequency;
        public int cycles;
        public int period;

        public bool lenable;
        public bool renable;

        public byte ReadRegister1(uint address) { return registers[0]; }

        public byte ReadRegister2(uint address) { return registers[1]; }

        public byte ReadRegister3(uint address) { return registers[2]; }

        public byte ReadRegister4(uint address) { return registers[3]; }

        public byte ReadRegister5(uint address) { return registers[4]; }

        public byte ReadRegister6(uint address) { return registers[5]; }

        public byte ReadRegister7(uint address) { return registers[6]; }

        public byte ReadRegister8(uint address) { return registers[7]; }

        public void base_WriteRegister1(uint address, byte data) { registers[0] = data; }

        public void base_WriteRegister2(uint address, byte data) { registers[1] = data; }

        public void base_WriteRegister3(uint address, byte data) { registers[2] = data; }

        public void base_WriteRegister4(uint address, byte data) { registers[3] = data; }

        public void base_WriteRegister5(uint address, byte data) { registers[4] = data; }

        public void base_WriteRegister6(uint address, byte data) { registers[5] = data; }

        public void base_WriteRegister7(uint address, byte data) { registers[6] = data; }

        public void base_WriteRegister8(uint address, byte data) { registers[7] = data; }

        public void ClockDuration()
        {
            if (duration.Clock())
            {
                active = false;
            }
        }
    }
}
