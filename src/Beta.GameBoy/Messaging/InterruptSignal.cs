namespace Beta.GameBoy.Messaging
{
    public sealed class InterruptSignal
    {
        public readonly int Flag;

        public InterruptSignal(int flag)
        {
            this.Flag = flag;
        }
    }
}
