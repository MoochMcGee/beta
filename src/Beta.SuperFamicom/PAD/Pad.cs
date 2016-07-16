using Beta.Platform.Input;
using Beta.Platform.Messaging;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.PAD
{
    public sealed class Pad : InputBackend, IConsumer<FrameSignal>
    {
        private readonly State state;
        private readonly int index;

        public Pad(State state, int index)
            : base(index, 12)
        {
            this.state = state;
            this.index = index;

            Map(0, "A");
            Map(1, "X");
            Map(2, "Back");
            Map(3, "Menu");
            Map(4, "DPad-U");
            Map(5, "DPad-D");
            Map(6, "DPad-L");
            Map(7, "DPad-R");
            Map(8, "B");
            Map(9, "Y");
            Map(10, "L-Shoulder");
            Map(11, "R-Shoulder");
        }

        public void Consume(FrameSignal e)
        {
            Update();

            ushort latch = 0x0000;

            if (Pressed(0x0)) latch |= 0x8000;
            if (Pressed(0x1)) latch |= 0x4000;
            if (Pressed(0x2)) latch |= 0x2000;
            if (Pressed(0x3)) latch |= 0x1000;
            if (Pressed(0x4)) latch |= 0x0800;
            if (Pressed(0x5)) latch |= 0x0400;
            if (Pressed(0x6)) latch |= 0x0200;
            if (Pressed(0x7)) latch |= 0x0100;
            if (Pressed(0x8)) latch |= 0x0080;
            if (Pressed(0x9)) latch |= 0x0040;
            if (Pressed(0xa)) latch |= 0x0020;
            if (Pressed(0xb)) latch |= 0x0010;

            state.pads[index] = latch;
        }
    }
}
