namespace Beta.GameBoy.Messaging
{
    public sealed class InterruptSignal
    {
        public byte Flag { get; }

        public InterruptSignal(byte flag)
        {
            this.Flag = flag;
        }
    }
}
