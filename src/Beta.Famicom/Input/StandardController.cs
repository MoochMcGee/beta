using Beta.Platform.Input;

namespace Beta.Famicom.Input
{
    public class StandardController : IJoypad
    {
        private HostInputDevice input;
        private int latch;
        private int value;

        public StandardController(int index)
        {
            input = new HostInputDevice(index, 10);
            input.Map(HostInputButton.A, 0);
            input.Map(HostInputButton.X, 1);
            input.Map(HostInputButton.Select, 2);
            input.Map(HostInputButton.Start, 3);
            input.Map(HostInputButton.DPadUp, 4);
            input.Map(HostInputButton.DPadDown, 5);
            input.Map(HostInputButton.DPadLeft, 6);
            input.Map(HostInputButton.DPadRight, 7);
        }

        public byte getData(int strobe)
        {
            var temp = value;

            if (strobe == 0)
            {
                value = (value >> 1) | 0x80;
            }

            return (byte)(temp & 1);
        }

        public void setData()
        {
            value = latch;
        }

        public void update()
        {
            input.Update();

            latch = 0;

            if (input.Pressed(0)) latch |= 0x01;
            if (input.Pressed(1)) latch |= 0x02;
            if (input.Pressed(2)) latch |= 0x04;
            if (input.Pressed(3)) latch |= 0x08;
            if (input.Pressed(4)) latch |= 0x10;
            if (input.Pressed(5)) latch |= 0x20;
            if (input.Pressed(6)) latch |= 0x40;
            if (input.Pressed(7)) latch |= 0x80;
        }
    }
}
