using Beta.Famicom.Messaging;

namespace Beta.Famicom.Input
{
    public sealed class InputConnector
    {
        private IJoypad joypad1;
        private IJoypad joypad2;
        private int strobe;

        public void ConnectJoypad1(IJoypad joypad)
        {
            this.joypad1 = joypad;
        }

        public void ConnectJoypad2(IJoypad joypad)
        {
            this.joypad2 = joypad;
        }

        public byte ReadJoypad1()
        {
            return joypad1.getData(strobe);
        }

        public byte ReadJoypad2()
        {
            return joypad2.getData(strobe);
        }

        public void Write(byte data)
        {
            strobe = (data & 1);

            if (strobe == 0)
            {
                joypad1.setData();
                joypad2.setData();
            }
        }

        public void Consume(FrameSignal e)
        {
            joypad1.update();
            joypad2.update();
        }
    }
}
