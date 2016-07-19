using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Input
{
    public sealed class InputConnector : IConsumer<FrameSignal>
    {
        private Joypad joypad1;
        private Joypad joypad2;
        private int strobe;

        public void ConnectJoypad1(Joypad joypad)
        {
            this.joypad1 = joypad;
        }

        public void ConnectJoypad2(Joypad joypad)
        {
            this.joypad2 = joypad;
        }

        public byte ReadJoypad1()
        {
            return joypad1.GetData(strobe);
        }

        public byte ReadJoypad2()
        {
            return joypad2.GetData(strobe);
        }

        public void Write(byte data)
        {
            strobe = (data & 1);

            if (strobe == 0)
            {
                joypad1.SetData();
                joypad2.SetData();
            }
        }

        public void Consume(FrameSignal e)
        {
            Joypad.AutofireState = !Joypad.AutofireState;

            joypad1.Update();
            joypad2.Update();
        }
    }
}
