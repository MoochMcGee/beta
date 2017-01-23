namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelPCM
    {
        private readonly Dma dma;

        private sbyte[] fifo = new sbyte[32];
        private int sz;
        private int rd;
        private int wr;

        public bool[] output = new bool[2];

        public int level;
        public int shift;
        public int timer;

        public ChannelPCM(Dma dma)
        {
            this.dma = dma;
        }

        public void Clock()
        {
            if (sz > 0)
            {
                level = fifo[rd] << 1;
                rd = (rd + 1) & 31;
                sz = (sz - 1);
            }

            if (sz > 16) return;

            if (dma.Enabled && dma.Type == Dma.SPECIAL)
            {
                dma.Pending = true;
            }
        }

        public void Reset()
        {
            sz = 0;
            rd = 0;
            wr = 0;
        }

        public void Write(uint address, byte data)
        {
            if (sz < 32)
            {
                fifo[wr] = (sbyte)data;
                wr = (wr + 1) & 31;
                sz = (sz + 1);
            }
        }
    }
}
