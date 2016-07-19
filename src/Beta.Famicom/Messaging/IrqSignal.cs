namespace Beta.Famicom.Messaging
{
    public sealed class IrqSignal
    {
        public int Value { get; }

        public IrqSignal(int value)
        {
            this.Value = value;
        }
    }
}
