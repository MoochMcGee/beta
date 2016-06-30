namespace Beta.Famicom.Messaging
{
    public sealed class VblNmiSignal
    {
        public int Value { get; }

        public VblNmiSignal(int value)
        {
            this.Value = value;
        }
    }
}
