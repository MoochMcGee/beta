using SharpDX.XInput;

namespace Beta.Platform.Input
{
    public class InputBackend
    {
        private readonly Controller controller;
        private readonly GamepadButtonFlags[] buttons;

        private State state;

        protected InputBackend(int index, int numberOfButtons)
        {
            controller = new Controller((UserIndex)index);
            buttons = new GamepadButtonFlags[numberOfButtons];
        }

        protected void Map(int index, string button)
        {
            switch (button)
            {
            case "A": buttons[index] = GamepadButtonFlags.A; break;
            case "B": buttons[index] = GamepadButtonFlags.B; break;
            case "X": buttons[index] = GamepadButtonFlags.X; break;
            case "Y": buttons[index] = GamepadButtonFlags.Y; break;

            case "Back": buttons[index] = GamepadButtonFlags.Back; break;
            case "Menu": buttons[index] = GamepadButtonFlags.Start; break;

            case "DPad-U": buttons[index] = GamepadButtonFlags.DPadUp; break;
            case "DPad-D": buttons[index] = GamepadButtonFlags.DPadDown; break;
            case "DPad-L": buttons[index] = GamepadButtonFlags.DPadLeft; break;
            case "DPad-R": buttons[index] = GamepadButtonFlags.DPadRight; break;

            case "L-Shoulder": buttons[index] = GamepadButtonFlags.LeftShoulder; break;
            case "R-Shoulder": buttons[index] = GamepadButtonFlags.RightShoulder; break;
            }
        }

        protected bool Pressed(int index)
        {
            return (state.Gamepad.Buttons & buttons[index]) != 0;
        }

        public virtual void Initialize() { }

        public virtual void Update()
        {
            if (controller.IsConnected)
            {
                state = controller.GetState();
            }
        }
    }
}
