using Beta.Platform.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public sealed class Dma
    {
        private readonly IProducer<ClockSignal> clock;
        private readonly SCpuState scpu;

        public BusA Bus;

        public Dma(IProducer<ClockSignal> clock, State state)
        {
            this.clock = clock;
            this.scpu = state.scpu;
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
                var enable = (scpu.mdma_en & (1 << i)) != 0;
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
            var channel = scpu.dma[i];
            var step = 0;

            while (true)
            {
                clock.Produce(new ClockSignal(8));
                amount += 8;

                var bank = (byte)(channel.address_a >> 16);
                var addr = (ushort)(channel.address_a >> 0);
                var dest = GetAddressB(channel, step);

                byte data = 0;

                if ((channel.control & 0x80) == 0)
                {
                    Bus.Read(bank, addr, ref data);
                    Bus.Write(0, dest, data);
                }
                else
                {
                    Bus.Read(0, dest, ref data);
                    Bus.Write(bank, addr, data);
                }

                switch ((channel.control >> 3) & 3)
                {
                case 0: channel.address_a++; break;
                case 1: break;
                case 2: channel.address_a--; break;
                case 3: break;
                }

                step++;

                channel.count--;

                if (channel.count == 0)
                {
                    break;
                }
            }

            return amount;
        }

        private ushort GetAddressB(DmaState channel, int step)
        {
            int init = channel.address_b;
            int type = channel.control & 7;

            switch (type)
            {
            case 0: step = 0; break;
            case 1: step = (step & 1); break;
            case 2: step = 0; break;
            case 3: step = (step & 2) / 2; break;
            case 4: step = (step & 3); break;
            case 5: step = (step & 1); break;
            case 6: step = 0; break;
            case 7: step = (step & 2) / 2; break;
            }

            return (ushort)(0x2100 | ((init + step) & 0xff));
        }
    }
}
