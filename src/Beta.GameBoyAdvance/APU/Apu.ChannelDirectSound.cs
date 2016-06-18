using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu
    {
        public class ChannelDirectSound : Channel
        {
            private Dma channel;
            private sbyte[] array;
            private int count;
            private int index;
            private int write;

            public int Level;
            public int Shift;
            public int Timer;

            public ChannelDirectSound(GameSystem gameSystem, Timing timing)
                : base(gameSystem, timing)
            {
                array = new sbyte[32];
            }

            private void PokeFifo(uint address, byte data)
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
                base.Initialize();

                channel = dma;

                gameSystem.mmio.Map(address + 0, PokeFifo);
                gameSystem.mmio.Map(address + 1, PokeFifo);
                gameSystem.mmio.Map(address + 2, PokeFifo);
                gameSystem.mmio.Map(address + 3, PokeFifo);
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
}
