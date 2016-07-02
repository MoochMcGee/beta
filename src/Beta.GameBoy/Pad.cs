using Beta.Platform.Input;

namespace Beta.GameBoy
{
    public class Pad : InputBackend
    {
        public static bool AutofireState;

        private bool p14;
        private bool p15;
        private byte p14Latch;
        private byte p15Latch;

        public Pad(IAddressSpace addressSpace)
            : base(0, 10)
        {
            addressSpace.Map(0xff00, Read, Write);

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

        private byte Read(ushort address)
        {
            if (p15) { return p15Latch; }
            if (p14) { return p14Latch; }

            return 0xff;
        }

        private void Write(ushort address, byte data)
        {
            p15 = (data & 0x20) == 0;
            p14 = (data & 0x10) == 0;
        }

        public override void Update()
        {
            base.Update();

            p15Latch = 0xff ^ 0x20;

            if (Pressed(0)) p15Latch ^= 0x1;
            if (Pressed(1)) p15Latch ^= 0x2;
            if (Pressed(2)) p15Latch ^= 0x4;
            if (Pressed(3)) p15Latch ^= 0x8;

            p14Latch = 0xff ^ 0x10;

            if (Pressed(4)) p14Latch ^= 0x1;
            if (Pressed(5)) p14Latch ^= 0x2;
            if (Pressed(6)) p14Latch ^= 0x4;
            if (Pressed(7)) p14Latch ^= 0x8;

            if (AutofireState)
            {
                if (Pressed(8)) p15Latch ^= 0x1;
                if (Pressed(9)) p15Latch ^= 0x2;
            }
        }
    }
}
