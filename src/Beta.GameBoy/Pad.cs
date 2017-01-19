using Beta.GameBoy.Messaging;
using Beta.Platform.Input;

namespace Beta.GameBoy
{
    public class Pad : InputBackend
    {
        private readonly State regs;

        public Pad(State regs)
            : base(0, 10)
        {
            this.regs = regs;

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

            regs.pad.p15_latch = 0xff ^ 0x20;

            if (Pressed(0)) regs.pad.p15_latch ^= 0x1;
            if (Pressed(1)) regs.pad.p15_latch ^= 0x2;
            if (Pressed(2)) regs.pad.p15_latch ^= 0x4;
            if (Pressed(3)) regs.pad.p15_latch ^= 0x8;

            regs.pad.p14_latch = 0xff ^ 0x10;

            if (Pressed(4)) regs.pad.p14_latch ^= 0x1;
            if (Pressed(5)) regs.pad.p14_latch ^= 0x2;
            if (Pressed(6)) regs.pad.p14_latch ^= 0x4;
            if (Pressed(7)) regs.pad.p14_latch ^= 0x8;
        }
    }
}
