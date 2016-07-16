namespace Beta.SuperFamicom.Messaging
{
    public sealed class VBlankSignal
    {
        public bool VBlank { get; }

        public VBlankSignal(bool vblank)
        {
            this.VBlank = vblank;
        }
    }
}
