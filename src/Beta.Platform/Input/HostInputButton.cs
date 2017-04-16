using SharpDX.XInput;

namespace Beta.Platform.Input
{
    public enum HostInputButton
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
}
