namespace Beta.Famicom.Input
{
    public class StandardController : Joypad
    {
        private byte latch;
        private byte value;

        public StandardController(int index)
            : base(index, 10)
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
        }

        public override byte GetData(int strobe)
        {
            var temp = value;

            if (strobe == 0)
            {
                value >>= 1;
                value |= 0x80;
            }

            return (byte)(temp & 1);
        }

        public override void SetData()
        {
            value = latch;
        }

        public override void Update()
        {
            base.Update();

            latch = 0;

            if (Pressed(0)) latch |= 0x01;
            if (Pressed(1)) latch |= 0x02;
            if (Pressed(2)) latch |= 0x04;
            if (Pressed(3)) latch |= 0x08;
            if (Pressed(4)) latch |= 0x10;
            if (Pressed(5)) latch |= 0x20;
            if (Pressed(6)) latch |= 0x40;
            if (Pressed(7)) latch |= 0x80;

            if (AutofireState)
                return;

            // autofire buttons
            if (Pressed(8)) latch |= 0x01;
            if (Pressed(9)) latch |= 0x02;
        }
    }
}
