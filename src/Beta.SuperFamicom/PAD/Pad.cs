using Beta.Platform;
using Beta.Platform.Input;

namespace Beta.SuperFamicom.PAD
{
    public sealed class Pad : InputBackend
    {
        public Register16 Latch;

        public Pad(int index)
            : base(index, 12)
        {
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

        public override void Update()
        {
            base.Update();

            Latch.w = 0x0000;

            if (Pressed(0x0)) Latch.w |= 0x8000;
            if (Pressed(0x1)) Latch.w |= 0x4000;
            if (Pressed(0x2)) Latch.w |= 0x2000;
            if (Pressed(0x3)) Latch.w |= 0x1000;
            if (Pressed(0x4)) Latch.w |= 0x0800;
            if (Pressed(0x5)) Latch.w |= 0x0400;
            if (Pressed(0x6)) Latch.w |= 0x0200;
            if (Pressed(0x7)) Latch.w |= 0x0100;
            if (Pressed(0x8)) Latch.w |= 0x0080;
            if (Pressed(0x9)) Latch.w |= 0x0040;
            if (Pressed(0xA)) Latch.w |= 0x0020;
            if (Pressed(0xB)) Latch.w |= 0x0010;
        }
    }
}
