using Beta.GameBoyAdvance.Memory;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ChannelPCM : Channel
    {
        private Dma channel;
        private sbyte[] array;
        private int count;
        private int index;
        private int write;

        public int Level;
        public int Shift;
        public int Timer;

        public ChannelPCM(MMIO mmio)
            : base(mmio)
        {
            array = new sbyte[32];

            cycles =
            period = Apu.Frequency;
        }

        private void WriteFifo(uint address, byte data)
        {
            if (count < 32)
            {
                array[write] = (sbyte)data;
                write = (write + 1) & 31;
                count = (count + 1);
            }
        }

        public void Initialize(Dma dma, uint address)
        {
            channel = dma;

            mmio.Map(address + 0, WriteFifo);
            mmio.Map(address + 1, WriteFifo);
            mmio.Map(address + 2, WriteFifo);
            mmio.Map(address + 3, WriteFifo);
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

            if (channel.Enabled && channel.Type == Dma.SPECIAL)
            {
                channel.Pending = true;
            }
        }
    }
}
