using Beta.GameBoy.Messaging;
using Beta.Platform.Input;

namespace Beta.GameBoy
{
    public class Pad : InputBackend
    {
        private readonly PadState regs;

        public Pad(State regs)
            : base(0, 10)
        {
            this.regs = regs.pad;

            Map(0, "A");
            Map(1, "X");
            Map(2, "Back");
            Map(3, "Menu");
            Map(4, "DPad-R");
            Map(5, "DPad-L");
            Map(6, "DPad-U");
            Map(7, "DPad-D");
            Map(8, "B");
            Map(9, "Y");
        }

        public void Consume(FrameSignal e)
        {
            base.Update();

            regs.p15_latch = 0xff ^ 0x20;

            if (Pressed(0)) regs.p15_latch ^= 1;
            if (Pressed(1)) regs.p15_latch ^= 2;
            if (Pressed(2)) regs.p15_latch ^= 4;
            if (Pressed(3)) regs.p15_latch ^= 8;

            regs.p14_latch = 0xff ^ 0x10;

            if (Pressed(4)) regs.p14_latch ^= 1;
            if (Pressed(5)) regs.p14_latch ^= 2;
            if (Pressed(6)) regs.p14_latch ^= 4;
            if (Pressed(7)) regs.p14_latch ^= 8;
        }
    }
}
