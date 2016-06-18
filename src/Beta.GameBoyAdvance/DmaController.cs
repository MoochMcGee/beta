namespace Beta.GameBoyAdvance
{
    public sealed class DmaController
    {
        public Dma[] Channels;

        public void VBlank()
        {
            foreach (var channel in Channels)
            {
                if (channel.Enabled && channel.Type == Dma.V_BLANK)
                {
                    channel.Pending = true;
                }
            }
        }

        public void HBlank()
        {
            foreach (var channel in Channels)
            {
                if (channel.Enabled && channel.Type == Dma.H_BLANK)
                {
                    channel.Pending = true;
                }
            }
        }

        public void Transfer()
        {
            foreach (var channel in Channels)
            {
                if (channel.Enabled && channel.Pending)
                {
                    channel.Pending = false;
                    channel.Transfer();
                }
            }
        }
    }
}
