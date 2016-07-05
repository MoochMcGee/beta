namespace Beta.GameBoyAdvance.Messaging
{
    public sealed class InterruptSignal
    {
        public ushort Flag { get; }

        public InterruptSignal(ushort flag)
        {
            this.Flag = flag;
        }
    }
}
