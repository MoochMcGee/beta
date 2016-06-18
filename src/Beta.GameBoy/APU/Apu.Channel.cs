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

            private GameSystem gameSystem;
            private byte[] emptyBits;
            private bool power;

            protected Duration Duration = new Duration();
            protected Envelope Envelope = new Envelope();
            protected byte[] Registers = new byte[5];
            protected Timing Timing;
            protected bool Active;
            protected int Frequency;

            public bool Enabled { get { return Active; } }

            protected Channel(GameSystem gameSystem, int channel)
            {
                this.gameSystem = gameSystem;
                emptyBits = registerTable[channel];
            }

            protected virtual void OnPokeReg1(byte data)
            {
            }

            protected virtual void OnPokeReg2(byte data)
            {
            }

            protected virtual void OnPokeReg3(byte data)
            {
            }

            protected virtual void OnPokeReg4(byte data)
            {
            }

            protected virtual void OnPokeReg5(byte data)
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

                PokeReg1(0x0000, 0x00);
                PokeReg2(0x0000, 0x00);
                PokeReg3(0x0000, 0x00);
                PokeReg4(0x0000, 0x00);
                PokeReg5(0x0000, 0x00);

                Active = false;
                power = false;
            }

            public void PowerOn()
            {
                power = true;
            }

            private byte PeekReg1(uint address)
            {
                return (byte)(Registers[0] | emptyBits[0]);
            }

            private byte PeekReg2(uint address)
            {
                return (byte)(Registers[1] | emptyBits[1]);
            }

            private byte PeekReg3(uint address)
            {
                return (byte)(Registers[2] | emptyBits[2]);
            }

            private byte PeekReg4(uint address)
            {
                return (byte)(Registers[3] | emptyBits[3]);
            }

            private byte PeekReg5(uint address)
            {
                return (byte)(Registers[4] | emptyBits[4]);
            }

            private void PokeReg1(uint address, byte data)
            {
                if (power)
                {
                    OnPokeReg1(Registers[0] = data);
                }
            }

            private void PokeReg2(uint address, byte data)
            {
                if (power)
                {
                    OnPokeReg2(Registers[1] = data);
                }
            }

            private void PokeReg3(uint address, byte data)
            {
                if (power)
                {
                    OnPokeReg3(Registers[2] = data);
                }
            }

            private void PokeReg4(uint address, byte data)
            {
                if (power)
                {
                    OnPokeReg4(Registers[3] = data);
                }
            }

            private void PokeReg5(uint address, byte data)
            {
                if (power)
                {
                    OnPokeReg5(Registers[4] = data);
                }
            }

            public virtual void Initialize()
            {
            }

            public void Initialize(uint address)
            {
                Initialize();

                gameSystem.Hook(address + 0U, PeekReg1, PokeReg1);
                gameSystem.Hook(address + 1U, PeekReg2, PokeReg2);
                gameSystem.Hook(address + 2U, PeekReg3, PokeReg3);
                gameSystem.Hook(address + 3U, PeekReg4, PokeReg4);
                gameSystem.Hook(address + 4U, PeekReg5, PokeReg5);
            }
        }
    }
}
