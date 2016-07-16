namespace Beta.SuperFamicom.Messaging
{
    public sealed class HBlankSignal
    {
        public bool HBlank { get; }

        public HBlankSignal(bool hblank)
        {
            this.HBlank = hblank;
        }
    }
}
