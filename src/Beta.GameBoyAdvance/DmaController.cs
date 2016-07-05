using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public sealed class DmaController
        : IConsumer<HBlankSignal>
        , IConsumer<VBlankSignal>
    {
        public Dma[] Channels;

        public DmaController(IMemoryMap memory, MMIO mmio, IProducer<InterruptSignal> interrupt)
        {
            Channels = new[]
            {
                new Dma(memory, mmio, interrupt, Cpu.Source.Dma0),
                new Dma(memory, mmio, interrupt, Cpu.Source.Dma1),
                new Dma(memory, mmio, interrupt, Cpu.Source.Dma2),
                new Dma(memory, mmio, interrupt, Cpu.Source.Dma3)
            };

            Channels[0].Initialize(0x0b0);
            Channels[1].Initialize(0x0bc);
            Channels[2].Initialize(0x0c8);
            Channels[3].Initialize(0x0d4);
        }

        public void Consume(VBlankSignal e)
        {
            foreach (var channel in Channels)
            {
                if (channel.Enabled && channel.Type == Dma.V_BLANK)
                {
                    channel.Pending = true;
                }
            }
        }

        public void Consume(HBlankSignal e)
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
