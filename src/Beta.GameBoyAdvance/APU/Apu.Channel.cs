using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu
    {
        public abstract class Channel
        {
            protected readonly MMIO mmio;
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

            protected Channel(MMIO mmio, Timing timing)
            {
                this.mmio = mmio;
                this.timing = timing;
            }

            protected virtual byte ReadRegister1(uint address)
            {
                return registers[0];
            }

            protected virtual byte ReadRegister2(uint address)
            {
                return registers[1];
            }

            protected virtual byte ReadRegister3(uint address)
            {
                return registers[2];
            }

            protected virtual byte ReadRegister4(uint address)
            {
                return registers[3];
            }

            protected virtual byte ReadRegister5(uint address)
            {
                return registers[4];
            }

            protected virtual byte ReadRegister6(uint address)
            {
                return registers[5];
            }

            protected virtual byte ReadRegister7(uint address)
            {
                return registers[6];
            }

            protected virtual byte ReadRegister8(uint address)
            {
                return registers[7];
            }

            protected virtual void WriteRegister1(uint address, byte data)
            {
                registers[0] = data;
            }

            protected virtual void WriteRegister2(uint address, byte data)
            {
                registers[1] = data;
            }

            protected virtual void WriteRegister3(uint address, byte data)
            {
                registers[2] = data;
            }

            protected virtual void WriteRegister4(uint address, byte data)
            {
                registers[3] = data;
            }

            protected virtual void WriteRegister5(uint address, byte data)
            {
                registers[4] = data;
            }

            protected virtual void WriteRegister6(uint address, byte data)
            {
                registers[5] = data;
            }

            protected virtual void WriteRegister7(uint address, byte data)
            {
                registers[6] = data;
            }

            protected virtual void WriteRegister8(uint address, byte data)
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

                mmio.Map(address + 0, ReadRegister1, WriteRegister1);
                mmio.Map(address + 1, ReadRegister2, WriteRegister2);
                mmio.Map(address + 2, ReadRegister3, WriteRegister3);
                mmio.Map(address + 3, ReadRegister4, WriteRegister4);
                mmio.Map(address + 4, ReadRegister5, WriteRegister5);
                mmio.Map(address + 5, ReadRegister6, WriteRegister6);
                mmio.Map(address + 6, ReadRegister7, WriteRegister7);
                mmio.Map(address + 7, ReadRegister8, WriteRegister8);
            }
        }
    }
}
