using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-3")]
    public class KonamiVrc3 : Board
    {
        private Irq irq;
        private int[] prgPages;

        public KonamiVrc3(CartridgeImage image)
            : base(image)
        {
            prgPages = new int[2];
            prgPages[0] = +0 << 14;
            prgPages[1] = -1 << 14;

            irq = new Irq();
        }

        private void Poke8000(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0x000f) | ((data & 0x0f) << 0);
        }

        private void Poke9000(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0x00f0) | ((data & 0x0f) << 4);
        }

        private void PokeA000(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0x0f00) | ((data & 0x0f) << 8);
        }

        private void PokeB000(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0xf000) | ((data & 0x0f) << 12);
        }

        private void PokeC000(ushort address, ref byte data)
        {
            irq.Mode = (data & 4) != 0;
            irq.Enabled = (data & 2) != 0;
            irq.EnabledRefresh = (data & 1) != 0;

            if (irq.Enabled)
                irq.Counter = irq.Refresh;

            Cpu.Irq(0);
        }

        private void PokeD000(ushort address, ref byte data)
        {
            irq.Enabled = irq.EnabledRefresh;
            Cpu.Irq(0);
        }

        private void PokeE000(ushort address, ref byte data)
        {
        }

        private void PokeF000(ushort address, ref byte data)
        {
            prgPages[0] = (data & 0xf) << 14;
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x3fff) | prgPages[(address >> 14) & 1];
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Decode("1000 ---- ---- ----").Poke(Poke8000);
            bus.Decode("1001 ---- ---- ----").Poke(Poke9000);
            bus.Decode("1010 ---- ---- ----").Poke(PokeA000);
            bus.Decode("1011 ---- ---- ----").Poke(PokeB000);
            bus.Decode("1100 ---- ---- ----").Poke(PokeC000);
            bus.Decode("1101 ---- ---- ----").Poke(PokeD000);
            bus.Decode("1110 ---- ---- ----").Poke(PokeE000);
            bus.Decode("1111 ---- ---- ----").Poke(PokeF000);
        }

        public override void Consume(ClockSignal e)
        {
            if (!irq.Enabled)
            {
                return;
            }

            if (irq.Clock())
            {
                Cpu.Irq(1);
            }
        }

        private class Irq
        {
            public bool Mode;
            public bool Enabled;
            public bool EnabledRefresh;
            public int Counter;
            public int Refresh;

            public bool Clock()
            {
                if (Mode)
                {
                    if ((Counter & 0x00ff) == 0x00ff)
                    {
                        Counter = (Counter & ~0x00ff) | (Refresh & 0x00ff);
                        return true;
                    }

                    Counter = (Counter & ~0x00ff) | ((Counter + 1) & 0x00ff);
                    return false;
                }

                if ((Counter & 0xffff) == 0xffff)
                {
                    Counter = (Counter & ~0xffff) | (Refresh & 0xffff);
                    return true;
                }

                Counter = (Counter & ~0xffff) | ((Counter + 1) & 0xffff);
                return false;
            }
        }
    }
}
