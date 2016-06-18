using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private sealed class ChannelDmc : Channel
        {
            private static int[][] periodTable = new[]
            {
                new[] { 0x1ac, 0x17c, 0x154, 0x140, 0x11e, 0x0fe, 0x0e2, 0x0d6, 0x0be, 0x0a0, 0x08e, 0x080, 0x06a, 0x054, 0x048, 0x036 },
                new[] { 0x18e, 0x162, 0x13c, 0x12a, 0x114, 0x0ec, 0x0d2, 0x0c6, 0x0b0, 0x094, 0x084, 0x076, 0x062, 0x04e, 0x042, 0x032 }
            };

            private R2A03 cpu;
            private Output output;
            private Reader reader;
            private Register regs;

            public bool IrqPending;

            public bool Enabled
            {
                get { return reader.Size != 0; }
                set
                {
                    if (!value)
                    {
                        reader.Size = 0;
                    }
                    else if (reader.Size == 0)
                    {
                        reader.Addr = regs.Addr;
                        reader.Size = regs.Size;

                        if (!reader.Buffered)
                            DoDma();
                    }
                }
            }

            public ChannelDmc(R2A03 cpu)
            {
                Timing.Cycles =
                Timing.Period = periodTable[0][0] * PHASE;
                Timing.Single = PHASE * 2;
                this.cpu = cpu;
            }

            private void ClockOutput()
            {
                if (output.Active)
                {
                    var next = ((((output.Buffer << 2) & 4) - 2) + output.Dac) & 0xffff;
                    output.Buffer >>= 1;

                    if (next <= 0x7f)
                    {
                        output.Dac = next;
                    }
                }
            }

            private void ClockReader()
            {
                if (output.Shifter != 0)
                {
                    output.Shifter--;
                }
                else
                {
                    output.Shifter = 7;
                    output.Active = reader.Buffered;

                    if (output.Active)
                    {
                        output.Buffer = reader.Buffer;
                        reader.Buffered = false;

                        if (reader.Size != 0)
                        {
                            DoDma();
                        }
                    }
                }
            }

            private void DoDma()
            {
                reader.Buffer = cpu.Peek(reader.Addr);
                reader.Buffered = true;

                reader.Addr++;
                reader.Addr |= 0x8000;
                reader.Size--;

                if (reader.Size == 0)
                {
                    if ((regs.Ctrl & 0x40) != 0)
                    {
                        reader.Addr = regs.Addr;
                        reader.Size = regs.Size;
                    }
                    else if ((regs.Ctrl & 0x80) != 0)
                    {
                        IrqPending = true;
                        cpu.Irq(1);
                    }
                }
            }

            public void Clock()
            {
                Timing.Cycles -= Timing.Single;

                if (Timing.Cycles == 0)
                {
                    Timing.Cycles += Timing.Period;

                    ClockOutput();
                    ClockReader();
                }
            }

            public void PokeReg1(ushort address, ref byte data)
            {
                regs.Ctrl = data;
                Timing.Period = periodTable[0][regs.Ctrl & 0xf] * PHASE;

                if ((regs.Ctrl & 0x80) == 0)
                {
                    IrqPending = false;
                    cpu.Irq(0);
                }
            }

            public void PokeReg2(ushort address, ref byte data)
            {
                output.Dac = (data & 0x7f);
            }

            public void PokeReg3(ushort address, ref byte data)
            {
                regs.Addr = (ushort)((data << 6) | 0xc000);
            }

            public void PokeReg4(ushort address, ref byte data)
            {
                regs.Size = (ushort)((data << 4) | 0x0001);
            }

            public byte Render()
            {
                return (byte)output.Dac;
            }

            private struct Output
            {
                public bool Active;
                public int Buffer;
                public int Dac;
                public int Shifter;
            }

            private struct Reader
            {
                public bool Buffered;
                public ushort Addr;
                public ushort Size;
                public int Buffer;
            }

            private struct Register
            {
                public ushort Addr;
                public ushort Size;
                public int Ctrl;
            }
        }
    }
}
