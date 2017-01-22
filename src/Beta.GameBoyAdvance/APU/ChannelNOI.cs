using System;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelNOI
    {
        private static int[] divisorTable = new[]
        {
            0x08, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70
        };

        public Duration duration = new Duration();
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int cycles;
        public int period;

        public bool lenable;
        public bool renable;

        public int shift = 8;
        public int value = 0x6000;

        public ChannelNOI()
        {
            cycles =
            period = divisorTable[0] * 4 * Apu.Single;
        }

        // 4000078h - NR41 - Channel 4 Duration (R/W)
        //   Bit        Expl.
        //   0-5   -/W  Sound length; units of (64-n)/256s  (0-63)
        //   6-7   -/-  Not used
        //  The Length value is used only if Bit 6 in NR44 is set.

        // 4000079h - NR42 - Channel 4 Envelope (R/W)
        //   Bit        Expl.
        //   0-2   R/W  Envelope Step-Time; units of n/64s  (1-7, 0=No Envelope)
        //     3   R/W  Envelope Direction                  (0=Decrease, 1=Increase)
        //   4-7   R/W  Initial Volume of envelope          (1-15, 0=No Sound)

        // 400007Ah - Not Used
        // 400007Bh - Not Used

        // 400007Ch - SOUND4CNT_H (NR43, NR44) - Channel 4 Frequency/Control (R/W)
        //   Bit        Expl.
        //   0-2   R/W  Dividing Ratio of Frequencies (r)
        //     3   R/W  Counter Step/Width (0=15 bits, 1=7 bits)
        //   4-7   R/W  Shift Clock Frequency (s)
        //  Frequency = 524288 Hz / r / 2^(s+1) ;For r=0 assume r=0.5 instead

        // 400007Dh - Not Used
        //   Bit        Expl.
        //   0-5   -/-  Not used
        //     6   R/W  Length Flag  (1=Stop output when length in NR41 expires)
        //     7   -/W  Initial      (1=Restart Sound)

        // 400007Eh - Not Used
        // 400007Fh - Not Used

        public byte ReadRegister1(uint address) => registers[0];

        public byte ReadRegister2(uint address) => registers[1];

        public byte ReadRegister3(uint address) => registers[2];

        public byte ReadRegister4(uint address) => registers[3];

        public byte ReadRegister5(uint address) => registers[4];

        public byte ReadRegister6(uint address) => registers[5];

        public byte ReadRegister7(uint address) => registers[6];

        public byte ReadRegister8(uint address) => registers[7];

        public void WriteRegister1(uint address, byte data)
        {
            duration.Refresh = (data & 0x3F);
            duration.Counter = 64 - duration.Refresh;
        }

        public void WriteRegister2(uint address, byte data)
        {
            envelope.Level = (data >> 4 & 0xF);
            envelope.Delta = (data >> 2 & 0x2) - 1;
            envelope.Period = (data & 0x7);

            registers[1] = data;
        }

        public void WriteRegister3(uint address, byte data) { }

        public void WriteRegister4(uint address, byte data) { }

        public void WriteRegister5(uint address, byte data)
        {
            shift = data & 0x8;

            period = (divisorTable[data & 0x7] << (data >> 4)) * 4 * Apu.Single;

            registers[4] = data;
        }

        public void WriteRegister6(uint address, byte data)
        {
            if (data >= 0x80)
            {
                active = true;
                cycles = period;

                duration.Counter = 64 - duration.Refresh;
                envelope.Cycles = envelope.Period;
                envelope.CanUpdate = true;

                value = 0x4000 >> shift;
            }

            duration.Enabled = (data & 0x40) != 0;

            if ((registers[1] & 0xF8) == 0)
            {
                active = false;
            }

            registers[5] = data &= 0x40;
        }

        public void WriteRegister7(uint address, byte data) { }

        public void WriteRegister8(uint address, byte data) { }

        public void ClockDuration()
        {
            if (duration.Clock())
            {
                active = false;
            }
        }

        public void ClockEnvelope()
        {
            envelope.Clock();
        }

        public int Render(int t)
        {
            var sum = cycles;
            cycles -= t;

            if (active)
            {
                if (cycles >= 0)
                {
                    if ((value & 0x1) != 0)
                        return (byte)envelope.Level;
                }
                else
                {
                    if ((value & 0x1) == 0)
                        sum = 0;

                    for (; cycles < 0; cycles += period)
                    {
                        //int feedback = (((value >> 1) ^ value) & 1);
                        //value = ((value >> 1) | (feedback << xor));

                        if ((value & 0x1) != 0)
                        {
                            value = (value >> 1) ^ (0x6000 >> shift);
                            sum += Math.Min(-cycles, period);
                        }
                        else
                        {
                            value = (value >> 1);
                        }
                    }

                    return (byte)((sum * envelope.Level) / t);
                }
            }
            else
            {
                for (; cycles < 0; cycles += period)
                {
                    if ((value & 0x01) != 0)
                        value = (value >> 1) ^ (0x6000 >> shift);
                    else
                        value = (value >> 1);

                    //int feedback = (value ^ (value >> 1)) & 0x1;
                    //value = ((value >> 1) | (feedback << shift));
                }
            }

            return 0;
        }
    }
}
