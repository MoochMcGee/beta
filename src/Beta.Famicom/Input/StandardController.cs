namespace Beta.Famicom.Input
{
    public class StandardController : Joypad
    {
        private int latch;
        private int value;

        public StandardController(int index)
            : base(index, 10)
        {
            Map(HostButton.A, 0);
            Map(HostButton.X, 1);
            Map(HostButton.Select, 2);
            Map(HostButton.Start, 3);
            Map(HostButton.DPadUp, 4);
            Map(HostButton.DPadDown, 5);
            Map(HostButton.DPadLeft, 6);
            Map(HostButton.DPadRight, 7);
        }

        public override byte GetData(int strobe)
        {
            var temp = value;

            if (strobe == 0)
            {
                value = (value >> 1) | 0x80;
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
        }
    }
}
