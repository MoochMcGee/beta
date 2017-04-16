using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Input;

namespace Beta.GameBoyAdvance
{
    public sealed class Pad
    {
        private readonly HostInputDevice input;
        private readonly PadRegisters regs;

        public Pad(Registers regs)
        {
            this.input = new HostInputDevice(0, 10);
            this.regs = regs.pad;

            input.Map(HostInputButton.A, 0);             // 0 - Button A (0=Pressed, 1=Released)
            input.Map(HostInputButton.X, 1);             // 1 - Button B (etc.)
            input.Map(HostInputButton.Select, 2);        // 2 - Select   (etc.)
            input.Map(HostInputButton.Start, 3);         // 3 - Start    (etc.)
            input.Map(HostInputButton.DPadRight, 4);     // 4 - Right    (etc.)
            input.Map(HostInputButton.DPadLeft, 5);      // 5 - Left     (etc.)
            input.Map(HostInputButton.DPadUp, 6);        // 6 - Up       (etc.)
            input.Map(HostInputButton.DPadDown, 7);      // 7 - Down     (etc.)
            input.Map(HostInputButton.RightShoulder, 8); // 8 - Button R (etc.)
            input.Map(HostInputButton.LeftShoulder, 9);  // 9 - Button L (etc.)
        }

        public void Consume(FrameSignal e)
        {
            input.Update();

            regs.data = 0;

            if (input.Pressed(0)) regs.data |= 0x0001;
            if (input.Pressed(1)) regs.data |= 0x0002;
            if (input.Pressed(2)) regs.data |= 0x0004;
            if (input.Pressed(3)) regs.data |= 0x0008;
            if (input.Pressed(4)) regs.data |= 0x0010;
            if (input.Pressed(5)) regs.data |= 0x0020;
            if (input.Pressed(6)) regs.data |= 0x0040;
            if (input.Pressed(7)) regs.data |= 0x0080;
            if (input.Pressed(8)) regs.data |= 0x0100;
            if (input.Pressed(9)) regs.data |= 0x0200;

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
