using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;

namespace Beta.GameBoyAdvance
{
    public sealed class Timer
    {
        private const int RESOLUTION = 10;
        private const int OVERFLOW = (1 << 16) << RESOLUTION;

        private static int[] resolutionLut = new[]
        {
            10, // Log2( 1024 ) - Log2(    1 ),
             4, // Log2( 1024 ) - Log2(   64 ),
             2, // Log2( 1024 ) - Log2(  256 ),
             0  // Log2( 1024 ) - Log2( 1024 )
        };

        private Driver gameSystem;
        private Apu apu;
        private Cpu cpu;

        private ushort interruptType;
        private int control;
        private int counter;
        private int refresh;
        private int cycles;
        private int number;

        public Timer NextTimer;

        public bool Countup { get { return (control & 0x0084) == 0x0084; } }
        public bool Enabled { get { return (control & 0x0084) == 0x0080; } }

        public Timer(Driver gameSystem, ushort interruptType, int number)
        {
            this.gameSystem = gameSystem;
            this.interruptType = interruptType;
            this.number = number;
        }

        #region Registers

        private byte ReadCounter_0(uint address)
        {
            return (byte)(counter);
        }

        private byte ReadCounter_1(uint address)
        {
            return (byte)(counter >> 8);
        }

        private byte ReadControl_0(uint address)
        {
            return (byte)(control);
        }

        private byte ReadControl_1(uint address)
        {
            return (byte)(control >> 8);
        }

        private void WriteCounter_0(uint address, byte value)
        {
            refresh = (refresh & ~0x00ff) | (value << 0);
        }

        private void WriteCounter_1(uint address, byte value)
        {
            refresh = (refresh & ~0xff00) | (value << 8);
        }

        private void WriteControl_0(uint address, byte value)
        {
            control = (control & ~0x00ff) | (value << 0);
        }

        private void WriteControl_1(uint address, byte value)
        {
            control = (control & ~0xff00) | (value << 8);
        }

        #endregion

        private void Clock(int amount = 1 << RESOLUTION)
        {
            cycles += amount;

            if (cycles >= OVERFLOW)
            {
                cycles -= OVERFLOW;
                cycles += refresh << RESOLUTION;

                if (apu.DirectSound1.Timer == number) { apu.DirectSound1.Clock(); }
                if (apu.DirectSound2.Timer == number) { apu.DirectSound2.Clock(); }

                if ((control & 0x40) != 0)
                {
                    cpu.Interrupt(interruptType);
                }

                if (NextTimer != null && NextTimer.Countup)
                {
                    NextTimer.Update();
                }
            }

            counter = cycles >> RESOLUTION;
        }

        public void Initialize(uint address)
        {
            apu = gameSystem.Apu;
            cpu = gameSystem.Cpu;

            gameSystem.mmio.Map(address + 0u, ReadCounter_0, WriteCounter_0);
            gameSystem.mmio.Map(address + 1u, ReadCounter_1, WriteCounter_1);
            gameSystem.mmio.Map(address + 2u, ReadControl_0, WriteControl_0);
            gameSystem.mmio.Map(address + 3u, ReadControl_1, WriteControl_1);
        }

        public void Update()
        {
            Clock();
        }

        public void Update(int amount)
        {
            Clock(amount << resolutionLut[control & 3]);
        }
    }
}
