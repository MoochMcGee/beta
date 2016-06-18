using Beta.Platform.Input;

namespace Beta.Famicom.PAD
{
    public abstract class Pad : InputBackend
    {
        public static bool AutofireState;

        protected Pad(int index, int buttons)
            : base(index, buttons)
        {
        }

        public abstract byte GetData(int strobe);

        public abstract void SetData();

        public override void Initialize() { }
    }
}
