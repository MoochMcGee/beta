using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-4")]
    public class KonamiVrc4 : Board
    {
        private Irq irq;
        private int[] chrPages;
        private int[] prgPages;
        private int mirroring;
        private int prgMode;

        public KonamiVrc4(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[8];
            prgPages = new int[4];
            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = -2 << 13;
            prgPages[3] = -1 << 13;

            irq = new Irq();
        }

        private void Poke8000(ushort address, ref byte data)
        {
            prgPages[0] = (data & 0x1f) << 13;
        }

        private void Poke9000(ushort address, ref byte data)
        {
            mirroring = (data & 0x03);
        }

        private void Poke9002(ushort address, ref byte data)
        {
            prgMode = (data & 0x02) << 13;
        }

        private void PokeA000(ushort address, ref byte data)
        {
            prgPages[1] = (data & 0x1f) << 13;
        }

        private void PokeB000(ushort address, ref byte data)
        {
            chrPages[0] = (chrPages[0] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeB001(ushort address, ref byte data)
        {
            chrPages[0] = (chrPages[0] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeB002(ushort address, ref byte data)
        {
            chrPages[1] = (chrPages[1] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeB003(ushort address, ref byte data)
        {
            chrPages[1] = (chrPages[1] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeC000(ushort address, ref byte data)
        {
            chrPages[2] = (chrPages[2] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeC001(ushort address, ref byte data)
        {
            chrPages[2] = (chrPages[2] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeC002(ushort address, ref byte data)
        {
            chrPages[3] = (chrPages[3] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeC003(ushort address, ref byte data)
        {
            chrPages[3] = (chrPages[3] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeD000(ushort address, ref byte data)
        {
            chrPages[4] = (chrPages[4] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeD001(ushort address, ref byte data)
        {
            chrPages[4] = (chrPages[4] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeD002(ushort address, ref byte data)
        {
            chrPages[5] = (chrPages[5] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeD003(ushort address, ref byte data)
        {
            chrPages[5] = (chrPages[5] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeE000(ushort address, ref byte data)
        {
            chrPages[6] = (chrPages[6] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeE001(ushort address, ref byte data)
        {
            chrPages[6] = (chrPages[6] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeE002(ushort address, ref byte data)
        {
            chrPages[7] = (chrPages[7] & ~(0x0f << 10)) | ((data & 0x0f) << 10);
        }

        private void PokeE003(ushort address, ref byte data)
        {
            chrPages[7] = (chrPages[7] & ~(0xf0 << 10)) | ((data & 0x1f) << 14);
        }

        private void PokeF000(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0x0f) | ((data & 0x0f) << 0);
        }

        private void PokeF001(ushort address, ref byte data)
        {
            irq.Refresh = (irq.Refresh & ~0xf0) | ((data & 0x0f) << 4);
        }

        private void PokeF002(ushort address, ref byte data)
        {
            irq.Mode = (data & 4) != 0;
            irq.Enabled = (data & 2) != 0;
            irq.EnabledRefresh = (data & 1) != 0;
            irq.Scaler = 341;

            if (irq.Enabled)
                irq.Counter = irq.Refresh;

            Cpu.Irq(0);
        }

        private void PokeF003(ushort address, ref byte data)
        {
            irq.Enabled = irq.EnabledRefresh;
            Cpu.Irq(0);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x3ff) | chrPages[(address >> 10) & 7];
        }

        protected override int DecodePrg(ushort address)
        {
            address ^= (ushort)((~address << 1) & prgMode);

            return (address & 0x1fff) | prgPages[(address >> 13) & 3];
        }

        public override void Clock()
        {
            if (!irq.Enabled)
            {
                return;
            }

            if (irq.Mode)
            {
                if (irq.Clock())
                {
                    Cpu.Irq(1);
                }
            }
            else
            {
                irq.Scaler -= 3;

                if (irq.Scaler <= 0)
                {
                    irq.Scaler += 341;

                    if (irq.Clock())
                    {
                        Cpu.Irq(1);
                    }
                }
            }
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            var pin3 = 1 << int.Parse(GetPin("VRC4", 3).Replace("PRG A", ""));
            var pin4 = 1 << int.Parse(GetPin("VRC4", 4).Replace("PRG A", ""));

            bus.Decode("1000 ---- ---- ----").Poke(Poke8000);
            bus.Decode("1001 ---- ---- ----").Poke(Poke9000);
            bus.Decode("1010 ---- ---- ----").Poke(PokeA000);
            bus.Decode("1011 ---- ---- ----").Poke(PokeB000);
            bus.Decode("1100 ---- ---- ----").Poke(PokeC000);
            bus.Decode("1101 ---- ---- ----").Poke(PokeD000);
            bus.Decode("1110 ---- ---- ----").Poke(PokeE000);
            bus.Decode("1111 ---- ---- ----").Poke(PokeF000);

            if ((0 & 0x18) == pin4)
            {
                bus.Decode("1001 ---- ---- ----").Poke(Poke9000);
                bus.Decode("1011 ---- ---- ----").Poke(PokeB001);
                bus.Decode("1100 ---- ---- ----").Poke(PokeC001);
                bus.Decode("1101 ---- ---- ----").Poke(PokeD001);
                bus.Decode("1110 ---- ---- ----").Poke(PokeE001);
                bus.Decode("1111 ---- ---- ----").Poke(PokeF001);
            }

            if ((0 & 0x18) == pin3)
            {
                bus.Decode("1001 ---- ---- ----").Poke(Poke9002);
                bus.Decode("1011 ---- ---- ----").Poke(PokeB002);
                bus.Decode("1100 ---- ---- ----").Poke(PokeC002);
                bus.Decode("1101 ---- ---- ----").Poke(PokeD002);
                bus.Decode("1110 ---- ---- ----").Poke(PokeE002);
                bus.Decode("1111 ---- ---- ----").Poke(PokeF002);
            }

            bus.Decode("1001 ---- ---- ----").Poke(Poke9002);
            bus.Decode("1011 ---- ---- ----").Poke(PokeB003);
            bus.Decode("1100 ---- ---- ----").Poke(PokeC003);
            bus.Decode("1101 ---- ---- ----").Poke(PokeD003);
            bus.Decode("1110 ---- ---- ----").Poke(PokeE003);
            bus.Decode("1111 ---- ---- ----").Poke(PokeF003);
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            case 2: return 0;
            case 3: return 1;
            }

            throw new CompilerPleasingException();
        }

        private class Irq
        {
            public bool Mode;
            public bool Enabled;
            public bool EnabledRefresh;
            public int Counter;
            public int Refresh;
            public int Scaler;

            public bool Clock()
            {
                if (Counter == 0xff)
                {
                    Counter = Refresh;
                    return true;
                }

                Counter++;
                return false;
            }
        }
    }
}
