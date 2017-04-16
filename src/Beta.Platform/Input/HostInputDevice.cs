using SharpDX.XInput;

namespace Beta.Platform.Input
{
    public sealed class HostInputDevice
    {
        private readonly Controller controller;
        private readonly int[] hostButtons;

        private int state;

        public HostInputDevice(int index, int numberOfButtons)
        {
            controller = new Controller((UserIndex)index);
            hostButtons = new int[numberOfButtons];
        }

        public void Map(HostInputButton hostButton, int guestButton)
        {
            hostButtons[guestButton] = ((int)hostButton);
        }

        public bool Pressed(int index)
        {
            return (state & hostButtons[index]) != 0;
        }

        public void Update()
        {
            if (controller.IsConnected)
            {
                state = (int)controller.GetState().Gamepad.Buttons;
            }
        }
    }
}
