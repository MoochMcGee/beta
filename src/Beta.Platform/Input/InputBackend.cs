using SharpDX.XInput;

namespace Beta.Platform.Input
{
    public class InputBackend
    {
        protected enum HostButton
        {
            DPadUp = (int)GamepadButtonFlags.DPadUp,
            DPadDown = (int)GamepadButtonFlags.DPadDown,
            DPadLeft = (int)GamepadButtonFlags.DPadLeft,
            DPadRight = (int)GamepadButtonFlags.DPadRight,
            Start = (int)GamepadButtonFlags.Start,
            Select = (int)GamepadButtonFlags.Back,
            LeftShoulder = (int)GamepadButtonFlags.LeftShoulder,
            RightShoulder = (int)GamepadButtonFlags.RightShoulder,
            A = (int)GamepadButtonFlags.A,
            B = (int)GamepadButtonFlags.B,
            X = (int)GamepadButtonFlags.X,
            Y = (int)GamepadButtonFlags.Y,
        }

        private readonly Controller controller;
        private readonly GamepadButtonFlags[] buttons;

        private State state;

        protected InputBackend(int index, int numberOfButtons)
        {
            controller = new Controller((UserIndex)index);
            buttons = new GamepadButtonFlags[numberOfButtons];
        }

        protected void Map(HostButton hostButton, int guestButton)
        {
            buttons[guestButton] = (GamepadButtonFlags)((int)hostButton);
        }

        protected bool Pressed(int index)
        {
            return (state.Gamepad.Buttons & buttons[index]) != 0;
        }

        public virtual void Update()
        {
            if (controller.IsConnected)
            {
                state = controller.GetState();
            }
        }
    }
}
