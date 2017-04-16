using Beta.GameBoy.Messaging;
using Beta.Platform.Input;

namespace Beta.GameBoy
{
    public sealed class Pad
    {
        private readonly HostInputDevice input;
        private readonly PadState state;

        public Pad(State state)
        {
            this.input = new HostInputDevice(0, 10);
            this.state = state.pad;

            input.Map(HostInputButton.A, 0);
            input.Map(HostInputButton.X, 1);
            input.Map(HostInputButton.Select, 2);
            input.Map(HostInputButton.Start, 3);
            input.Map(HostInputButton.DPadRight, 4);
            input.Map(HostInputButton.DPadLeft, 5);
            input.Map(HostInputButton.DPadUp, 6);
            input.Map(HostInputButton.DPadDown, 7);
        }

        public void Consume(FrameSignal e)
        {
            input.Update();

            state.p15_latch = 0xff ^ 0x20;

            if (input.Pressed(0)) state.p15_latch ^= 1;
            if (input.Pressed(1)) state.p15_latch ^= 2;
            if (input.Pressed(2)) state.p15_latch ^= 4;
            if (input.Pressed(3)) state.p15_latch ^= 8;

            state.p14_latch = 0xff ^ 0x10;

            if (input.Pressed(4)) state.p14_latch ^= 1;
            if (input.Pressed(5)) state.p14_latch ^= 2;
            if (input.Pressed(6)) state.p14_latch ^= 4;
            if (input.Pressed(7)) state.p14_latch ^= 8;
        }
    }
}
