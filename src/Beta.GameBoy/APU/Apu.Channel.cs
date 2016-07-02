using Beta.Platform;

namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        private const int DELAY = 8192;
        private const int PHASE = 375;

        private class Channel
        {
            private static byte[][] registerTable = new[]
            {
                new byte[] { 0x80, 0x3f, 0x00, 0xff, 0xbf },
                new byte[] { 0xff, 0x3f, 0x00, 0xff, 0xbf },
                new byte[] { 0x7f, 0xff, 0x9f, 0xff, 0xbf },
                new byte[] { 0xff, 0xff, 0x00, 0x00, 0xbf }
            };

            private IAddressSpace addressSpace;
            private byte[] emptyBits;
            private bool power;

            protected Duration Duration = new Duration();
            protected Envelope Envelope = new Envelope();
            protected byte[] Registers = new byte[5];
            protected Timing Timing;
            protected bool Active;
            protected int Frequency;

            public bool Enabled { get { return Active; } }

            protected Channel(IAddressSpace addressSpace, int channel)
            {
                this.addressSpace = addressSpace;
                emptyBits = registerTable[channel];
            }

            protected virtual void OnWriteReg1(byte data)
            {
            }

            protected virtual void OnWriteReg2(byte data)
            {
            }

            protected virtual void OnWriteReg3(byte data)
            {
            }

            protected virtual void OnWriteReg4(byte data)
            {
            }

            protected virtual void OnWriteReg5(byte data)
            {
            }

            public void ClockDuration()
            {
                if (Duration.Clock())
                {
                    Active = false;
                }
            }

            public void PowerOff()
            {
                power = true;

                WriteReg1(0x0000, 0x00);
                WriteReg2(0x0000, 0x00);
                WriteReg3(0x0000, 0x00);
                WriteReg4(0x0000, 0x00);
                WriteReg5(0x0000, 0x00);

                Active = false;
                power = false;
            }

            public void PowerOn()
            {
                power = true;
            }

            private byte ReadReg1(ushort address)
            {
                return (byte)(Registers[0] | emptyBits[0]);
            }

            private byte ReadReg2(ushort address)
            {
                return (byte)(Registers[1] | emptyBits[1]);
            }

            private byte ReadReg3(ushort address)
            {
                return (byte)(Registers[2] | emptyBits[2]);
            }

            private byte ReadReg4(ushort address)
            {
                return (byte)(Registers[3] | emptyBits[3]);
            }

            private byte ReadReg5(ushort address)
            {
                return (byte)(Registers[4] | emptyBits[4]);
            }

            private void WriteReg1(ushort address, byte data)
            {
                if (power)
                {
                    OnWriteReg1(Registers[0] = data);
                }
            }

            private void WriteReg2(ushort address, byte data)
            {
                if (power)
                {
                    OnWriteReg2(Registers[1] = data);
                }
            }

            private void WriteReg3(ushort address, byte data)
            {
                if (power)
                {
                    OnWriteReg3(Registers[2] = data);
                }
            }

            private void WriteReg4(ushort address, byte data)
            {
                if (power)
                {
                    OnWriteReg4(Registers[3] = data);
                }
            }

            private void WriteReg5(ushort address, byte data)
            {
                if (power)
                {
                    OnWriteReg5(Registers[4] = data);
                }
            }

            public virtual void Initialize()
            {
            }

            public void Initialize(uint address)
            {
                Initialize();

                addressSpace.Map((ushort)(address + 0), ReadReg1, WriteReg1);
                addressSpace.Map((ushort)(address + 1), ReadReg2, WriteReg2);
                addressSpace.Map((ushort)(address + 2), ReadReg3, WriteReg3);
                addressSpace.Map((ushort)(address + 3), ReadReg4, WriteReg4);
                addressSpace.Map((ushort)(address + 4), ReadReg5, WriteReg5);
            }
        }
    }
}
