namespace Beta.Famicom.Messaging
{
    public sealed class VblSignal
    {
        public int Value { get; }

        public VblSignal(int value)
        {
            this.Value = value;
        }
    }
}
