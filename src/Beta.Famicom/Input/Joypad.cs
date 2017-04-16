using Beta.Platform.Input;

namespace Beta.Famicom.Input
{
    public abstract class Joypad : InputBackend
    {
        protected Joypad(int index, int buttons)
            : base(index, buttons)
        {
        }

        public abstract byte GetData(int strobe);

        public abstract void SetData();
    }
}
