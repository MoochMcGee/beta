namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelPCM
    {
        private Dma dma;
        private sbyte[] array;
        private int count;
        private int index;
        private int write;

        public bool enabled;
        public bool lenable;
        public bool renable;

        public int Level;
        public int Shift;
        public int Timer;

        public ChannelPCM()
        {
            array = new sbyte[32];
        }

        public void WriteFifo(uint address, byte data)
        {
            if (count < 32)
            {
                array[write] = (sbyte)data;
                write = (write + 1) & 31;
                count = (count + 1);
            }
        }

        public void Initialize(Dma dma)
        {
            this.dma = dma;
        }

        public void Clear()
        {
            for (var i = 0; i < 32; i++)
            {
                array[i] = 0;
            }

            count = 0;
            index = 0;
            write = 0;
        }

        public void Clock()
        {
            if (count > 0)
            {
                Level = array[index] << 1;
                index = (index + 1) & 31;
                count = (count - 1);
            }

            if (count > 16) return;

            if (dma.Enabled && dma.Type == Dma.SPECIAL)
            {
                dma.Pending = true;
            }
        }
    }
}
