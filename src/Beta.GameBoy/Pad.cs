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

            Map(HostButton.A, 0);
            Map(HostButton.X, 1);
            Map(HostButton.Select, 2);
            Map(HostButton.Start, 3);
            Map(HostButton.DPadRight, 4);
            Map(HostButton.DPadLeft, 5);
            Map(HostButton.DPadUp, 6);
            Map(HostButton.DPadDown, 7);
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
