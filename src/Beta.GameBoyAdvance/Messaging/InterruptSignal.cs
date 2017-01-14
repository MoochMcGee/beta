namespace Beta.GameBoyAdvance.Messaging
{
    public sealed class InterruptSignal
    {
        public readonly ushort Flag;

        public InterruptSignal(ushort flag)
        {
            this.Flag = flag;
        }
    }
}
