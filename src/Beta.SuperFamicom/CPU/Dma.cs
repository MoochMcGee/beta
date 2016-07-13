using Beta.Platform.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public sealed class Dma
    {
        private readonly IProducer<ClockSignal> clock;

        public DmaState[] dma;
        public byte mdma_en;
        public byte hdma_en;
        public int mdma_count;

        public BusA Bus;

        public Dma(IProducer<ClockSignal> clock, State state)
        {
            this.clock = clock;
            this.dma = state.scpu.dma;
        }

        public int Run(int totalCycles)
        {
            int amount = 0;
            int cycles = totalCycles & 7;
            if (cycles != 0)
            {
                // Align the clock divider
                clock.Produce(new ClockSignal(8 - cycles)); amount += 8 - cycles;
            }

            // DMA initialization
            clock.Produce(new ClockSignal(8)); amount += 8;

            for (int i = 0; i < 8; i++)
            {
                var enable = (mdma_en & (1 << i)) != 0;
                if (enable)
                {
                    // DMA channel initialization
                    clock.Produce(new ClockSignal(8)); amount += 8;
                    amount += RunChannel(i);
                }
            }

            return amount;
        }

        private int RunChannel(int i)
        {
            var amount = 0;
            var c = dma[i];
            var step = 0;

            while (true)
            {
                clock.Produce(new ClockSignal(8));
                amount += 8;

                if ((c.control & 0x80) == 0)
                {
                    var data = Bus.ReadFree(c.address_a.b, c.address_a.w);
                    var dest = GetAddressB(c.control & 7, c.address_b, step);
                    Bus.WriteFree(0, dest, data);
                }
                else
                {
                    var dest = GetAddressB(c.control & 7, c.address_b, step);
                    var data = Bus.ReadFree(0, dest);
                    Bus.WriteFree(c.address_a.b, c.address_a.w, data);
                }

                switch (c.control & 0x18)
                {
                case 0x00: c.address_a.d++; break;
                case 0x08: break;
                case 0x10: c.address_a.d--; break;
                case 0x18: break;
                }

                step++;

                if (--c.count == 0)
                {
                    break;
                }
            }

            return amount;
        }

        private ushort GetAddressB(int type, int init, int step)
        {
            int port = 0;

            switch (type)
            {
            case 0: port = init; break;
            case 1: port = init + ((step >> 0) & 1); break;
            case 2: port = init; break;
            case 3: port = init + ((step >> 1) & 1); break;
            case 4: port = init + ((step >> 0) & 3); break;
            case 5: port = init + ((step >> 0) & 1); break;
            case 6: port = init; break;
            case 7: port = init + ((step >> 1) & 1); break;
            }

            return (ushort)(0x2100 | (byte)port);
        }
    }
}
