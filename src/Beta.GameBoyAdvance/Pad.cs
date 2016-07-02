using Beta.GameBoyAdvance.CPU;
using Beta.Platform;
using Beta.Platform.Input;

namespace Beta.GameBoyAdvance
{
    public class Pad : InputBackend
    {
        public static bool AutofireState;

        private Driver gameSystem;
        private Cpu cpu;

        private Register16 data;
        private Register16 mask;

        public Pad(Driver gameSystem)
            : base(0, 10)
        {
            this.gameSystem = gameSystem;
            cpu = gameSystem.Cpu;

            Map(0, "A");          // 0 - Button A (0=Pressed, 1=Released)
            Map(1, "X");          // 1 - Button B (etc.)
            Map(2, "Back");       // 2 - Select   (etc.)
            Map(3, "Menu");       // 3 - Start    (etc.)
            Map(4, "DPad-R");     // 4 - Right    (etc.)
            Map(5, "DPad-L");     // 5 - Left     (etc.)
            Map(6, "DPad-U");     // 6 - Up       (etc.)
            Map(7, "DPad-D");     // 7 - Down     (etc.)
            Map(8, "R-Shoulder"); // 8 - Button R (etc.)
            Map(9, "L-Shoulder"); // 9 - Button L (etc.)
        }

        private byte Peek130(uint address)
        {
            return data.l;
        }

        private byte Peek131(uint address)
        {
            return data.h;
        }

        private byte Peek132(uint address)
        {
            return mask.l;
        }

        private byte Peek133(uint address)
        {
            return mask.h;
        }

        private void Poke132(uint address, byte data)
        {
            mask.l = data;
        }

        private void Poke133(uint address, byte data)
        {
            mask.h = data;
        }

        public void Initialize()
        {
            var mmio = gameSystem.mmio;
            mmio.Map(0x130, Peek130);
            mmio.Map(0x131, Peek131);
            mmio.Map(0x132, Peek132, Poke132);
            mmio.Map(0x133, Peek133, Poke133);
        }

        public override void Update()
        {
            base.Update();

            data.w = 0;

            if (Pressed(0)) data.w |= 0x0001;
            if (Pressed(1)) data.w |= 0x0002;
            if (Pressed(2)) data.w |= 0x0004;
            if (Pressed(3)) data.w |= 0x0008;
            if (Pressed(4)) data.w |= 0x0010;
            if (Pressed(5)) data.w |= 0x0020;
            if (Pressed(6)) data.w |= 0x0040;
            if (Pressed(7)) data.w |= 0x0080;
            if (Pressed(8)) data.w |= 0x0100;
            if (Pressed(9)) data.w |= 0x0200;

            if ((mask.w & 0x4000) != 0)
            {
                if ((mask.w & 0x8000) != 0)
                {
                    //if ((data.w & mask.w) != 0)
                    //{
                    //    cpu.Interrupt(Cpu.Source.JOYPAD);
                    //}
                }
                else
                {
                    //if ((data.w & mask.w) == mask.w)
                    //{
                    //    cpu.Interrupt(Cpu.Source.JOYPAD);
                    //}
                }
            }

            data.w ^= 0x3ff;
        }
    }
}
