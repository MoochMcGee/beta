using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu
    {
        public abstract class Channel
        {
            protected GameSystem gameSystem;
            protected Duration duration = new Duration();
            protected Envelope envelope = new Envelope();
            protected byte[] registers = new byte[8];

            protected Timing timing;
            protected bool active;
            protected int frequency;

            public bool lenable;
            public bool renable;

            public virtual bool Enabled
            {
                get { return active; }
                set { active = value; }
            }

            protected Channel(GameSystem gameSystem, Timing timing)
            {
                this.gameSystem = gameSystem;
                this.timing = timing;
            }

            protected virtual byte PeekRegister1(uint address)
            {
                return registers[0];
            }

            protected virtual byte PeekRegister2(uint address)
            {
                return registers[1];
            }

            protected virtual byte PeekRegister3(uint address)
            {
                return registers[2];
            }

            protected virtual byte PeekRegister4(uint address)
            {
                return registers[3];
            }

            protected virtual byte PeekRegister5(uint address)
            {
                return registers[4];
            }

            protected virtual byte PeekRegister6(uint address)
            {
                return registers[5];
            }

            protected virtual byte PeekRegister7(uint address)
            {
                return registers[6];
            }

            protected virtual byte PeekRegister8(uint address)
            {
                return registers[7];
            }

            protected virtual void PokeRegister1(uint address, byte data)
            {
                registers[0] = data;
            }

            protected virtual void PokeRegister2(uint address, byte data)
            {
                registers[1] = data;
            }

            protected virtual void PokeRegister3(uint address, byte data)
            {
                registers[2] = data;
            }

            protected virtual void PokeRegister4(uint address, byte data)
            {
                registers[3] = data;
            }

            protected virtual void PokeRegister5(uint address, byte data)
            {
                registers[4] = data;
            }

            protected virtual void PokeRegister6(uint address, byte data)
            {
                registers[5] = data;
            }

            protected virtual void PokeRegister7(uint address, byte data)
            {
                registers[6] = data;
            }

            protected virtual void PokeRegister8(uint address, byte data)
            {
                registers[7] = data;
            }

            public void ClockDuration()
            {
                if (duration.Clock())
                {
                    active = false;
                }
            }

            public virtual void Initialize()
            {
            }

            public void Initialize(uint address)
            {
                Initialize();

                gameSystem.mmio.Map(address + 0, PeekRegister1, PokeRegister1);
                gameSystem.mmio.Map(address + 1, PeekRegister2, PokeRegister2);
                gameSystem.mmio.Map(address + 2, PeekRegister3, PokeRegister3);
                gameSystem.mmio.Map(address + 3, PeekRegister4, PokeRegister4);
                gameSystem.mmio.Map(address + 4, PeekRegister5, PokeRegister5);
                gameSystem.mmio.Map(address + 5, PeekRegister6, PokeRegister6);
                gameSystem.mmio.Map(address + 6, PeekRegister7, PokeRegister7);
                gameSystem.mmio.Map(address + 7, PeekRegister8, PokeRegister8);
            }
        }
    }
}
