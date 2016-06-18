using System;

namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        //        Square 2
        //      FF15 ---- ---- Not used
        // NR21 FF16 DDLL LLLL Duty, Length load (64-L)
        // NR22 FF17 VVVV APPP Starting volume, Envelope add mode, period
        // NR23 FF18 FFFF FFFF Frequency LSB
        // NR24 FF19 TL-- -FFF Trigger, Length enable, Frequency MSB

        private sealed class ChannelSq2 : Channel
        {
            private static byte[][] dutyTable = new[]
            {
                new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
                new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x00 },
                new byte[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x00, 0x00, 0x00 },
                new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f }
            };

            private int form;
            private int step = 7;

            public ChannelSq2(GameSystem gameboy)
                : base(gameboy, 1)
            {
                Timing.Cycles =
                Timing.Single = PHASE * 2048;
                Timing.Period = DELAY;
            }

            protected override void OnPokeReg2(byte data)
            {
                form = data >> 6;
                Duration.Refresh = (data & 0x3F);
                Duration.Counter = 64 - Duration.Refresh;
            }

            protected override void OnPokeReg3(byte data)
            {
                Envelope.Level = (data >> 4 & 0xF);
                Envelope.Delta = (data >> 2 & 0x2) - 1;
                Envelope.Timing.Period = (data & 0x7);

                if ((Registers[2] & 0xF8) == 0)
                    Active = false;
            }

            protected override void OnPokeReg4(byte data)
            {
                Frequency = (Frequency & 0x700) | (data << 0 & 0x0FF);
                Timing.Single = (2048 - Frequency) * PHASE;
            }

            protected override void OnPokeReg5(byte data)
            {
                Frequency = (Frequency & 0x0FF) | (data << 8 & 0x700);
                Timing.Single = (2048 - Frequency) * PHASE;

                if ((data & 0x80) != 0)
                {
                    Active = true;
                    Timing.Cycles = Timing.Single;

                    if (Duration.Counter == 0)
                        Duration.Counter = 64;

                    Envelope.Timing.Cycles = Envelope.Timing.Period;
                    Envelope.CanUpdate = true;

                    step = 7;
                }

                Duration.Enabled = (data & 0x40) != 0;

                if ((Registers[2] & 0xF8) == 0)
                    Active = false;
            }

            public byte Sample()
            {
                var sum = Timing.Cycles;
                Timing.Cycles -= Timing.Period;

                if ((Registers[2] & 0xF8) != 0 && Active)
                {
                    if (Timing.Cycles >= 0)
                    {
                        return (byte)(Envelope.Level >> dutyTable[form][step]);
                    }

                    sum >>= dutyTable[form][step];

                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        sum += Math.Min(-Timing.Cycles, Timing.Single) >> dutyTable[form][step = (step - 1) & 0x7];
                    }

                    return (byte)((sum * Envelope.Level) / Timing.Period);
                }

                var count = (~Timing.Cycles + Timing.Single) / Timing.Single;

                step = (step - count) & 0x7;
                Timing.Cycles += (count * Timing.Single);

                return 0;
            }

            public void ClockEnvelope()
            {
                Envelope.Clock();
            }
        }
    }
}
