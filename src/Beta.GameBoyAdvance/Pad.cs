using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Input;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public class Pad : InputBackend, IConsumer<FrameSignal>
    {
        private readonly PadRegisters regs;

        public Pad(Registers regs)
            : base(0, 10)
        {
            this.regs = regs.pad;

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

        public void Consume(FrameSignal e)
        {
            base.Update();

            regs.data = 0;

            if (Pressed(0)) regs.data |= 0x0001;
            if (Pressed(1)) regs.data |= 0x0002;
            if (Pressed(2)) regs.data |= 0x0004;
            if (Pressed(3)) regs.data |= 0x0008;
            if (Pressed(4)) regs.data |= 0x0010;
            if (Pressed(5)) regs.data |= 0x0020;
            if (Pressed(6)) regs.data |= 0x0040;
            if (Pressed(7)) regs.data |= 0x0080;
            if (Pressed(8)) regs.data |= 0x0100;
            if (Pressed(9)) regs.data |= 0x0200;

            if ((regs.mask & 0x4000) != 0)
            {
                if ((regs.mask & 0x8000) != 0)
                {
                    // if ((data.w & mask.w) != 0)
                    // {
                    //     cpu.Interrupt(Cpu.Source.JOYPAD);
                    // }
                }
                else
                {
                    // if ((data.w & mask.w) == mask.w)
                    // {
                    //     cpu.Interrupt(Cpu.Source.JOYPAD);
                    // }
                }
            }

            regs.data ^= 0x3ff;
        }
    }
}
