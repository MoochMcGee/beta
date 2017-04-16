using Beta.Platform.Input;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.PAD
{
    public sealed class Pad
    {
        private readonly HostInputDevice input;
        private readonly State state;
        private readonly int index;

        public Pad(State state, int index)
        {
            this.input = new HostInputDevice(index, 12);
            this.state = state;
            this.index = index;

            input.Map(HostInputButton.A, 0);
            input.Map(HostInputButton.X, 1);
            input.Map(HostInputButton.Select, 2);
            input.Map(HostInputButton.Start, 3);
            input.Map(HostInputButton.DPadUp, 4);
            input.Map(HostInputButton.DPadDown, 5);
            input.Map(HostInputButton.DPadLeft, 6);
            input.Map(HostInputButton.DPadRight, 7);
            input.Map(HostInputButton.B, 8);
            input.Map(HostInputButton.Y, 9);
            input.Map(HostInputButton.LeftShoulder, 10);
            input.Map(HostInputButton.RightShoulder, 11);
        }

        public void Consume(FrameSignal e)
        {
            input.Update();

            ushort latch = 0x0000;

            if (input.Pressed(0x0)) latch |= 0x8000;
            if (input.Pressed(0x1)) latch |= 0x4000;
            if (input.Pressed(0x2)) latch |= 0x2000;
            if (input.Pressed(0x3)) latch |= 0x1000;
            if (input.Pressed(0x4)) latch |= 0x0800;
            if (input.Pressed(0x5)) latch |= 0x0400;
            if (input.Pressed(0x6)) latch |= 0x0200;
            if (input.Pressed(0x7)) latch |= 0x0100;
            if (input.Pressed(0x8)) latch |= 0x0080;
            if (input.Pressed(0x9)) latch |= 0x0040;
            if (input.Pressed(0xa)) latch |= 0x0020;
            if (input.Pressed(0xb)) latch |= 0x0010;

            state.pads[index] = latch;
        }
    }
}
