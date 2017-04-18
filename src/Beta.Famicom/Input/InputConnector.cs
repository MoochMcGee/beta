using Beta.Famicom.Messaging;

namespace Beta.Famicom.Input
{
    public static class InputConnector
    {
        private static IJoypad joypad1;
        private static IJoypad joypad2;
        private static int strobe;

        public static void ConnectJoypad1(IJoypad joypad)
        {
            InputConnector.joypad1 = joypad;
        }

        public static void ConnectJoypad2(IJoypad joypad)
        {
            InputConnector.joypad2 = joypad;
        }

        public static byte ReadJoypad1()
        {
            return joypad1.getData(strobe);
        }

        public static byte ReadJoypad2()
        {
            return joypad2.getData(strobe);
        }

        public static void Write(byte data)
        {
            strobe = (data & 1);

            if (strobe == 0)
            {
                joypad1.setData();
                joypad2.setData();
            }
        }

        public static void Consume(FrameSignal e)
        {
            joypad1.update();
            joypad2.update();
        }
    }
}
