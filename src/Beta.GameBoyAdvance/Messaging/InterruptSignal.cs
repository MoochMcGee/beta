using Beta.GameBoyAdvance.CPU;

namespace Beta.GameBoyAdvance.Messaging
{
    public sealed class InterruptSignal
    {
        public readonly Interrupt Flag;

        public InterruptSignal(Interrupt flag)
        {
            this.Flag = flag;
        }
    }
}
