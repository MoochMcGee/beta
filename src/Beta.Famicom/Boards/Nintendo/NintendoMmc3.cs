using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;

namespace Beta.Famicom.Boards.Nintendo
{
    [BoardName("KONAMI-TLROM")]
    [BoardName("HVC-TLROM")]
    [BoardName("NES-TGROM")]
    [BoardName("NES-TKROM")]
    [BoardName("NES-TKEPROM")]
    [BoardName("NES-TLROM")]
    [BoardName("NES-TSROM")]
    [BoardName("NES-TxROM")]
    public class NintendoMmc3 : Board
    {
        private int[] chrPages;
        private int[] prgPages;
        private bool ramEnabled;
        private bool ramProtect;
        private bool irqEnabled;
        private int irqCounter;
        private int irqRefresh;
        private int irqLatch;
        private int irqTimer;

        private int regAddr;
        private int chrMode;
        private int prgMode;
        private int mirroring;

        public NintendoMmc3(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[6];
            prgPages = new int[4];
        }

        private void Poke8000(ushort address, ref byte data)
        {
            chrMode = (data & 0x80) << 5; // $0000/$1000
            prgMode = (data & 0x40) << 8; // $0000/$4000
            regAddr = (data & 0x07) << 0;
        }

        private void Poke8001(ushort address, ref byte data)
        {
            switch (regAddr)
            {
            case 0: chrPages[0] = (data & ~1) << 10; break;
            case 1: chrPages[1] = (data & ~1) << 10; break;
            case 2: chrPages[2] = (data) << 10; break;
            case 3: chrPages[3] = (data) << 10; break;
            case 4: chrPages[4] = (data) << 10; break;
            case 5: chrPages[5] = (data) << 10; break;
            case 6: prgPages[0] = (data) << 13; break;
            case 7: prgPages[1] = (data) << 13; break;
            }
        }

        private void PokeA000(ushort address, ref byte data)
        {
            mirroring = (data & 0x01);
        }

        private void PokeA001(ushort address, ref byte data)
        {
            ramEnabled = (data & 0x80) != 0;
            ramProtect = (data & 0x40) != 0;
        }

        private void PokeC000(ushort address, ref byte data)
        {
            irqRefresh = data;
        }

        private void PokeC001(ushort address, ref byte data)
        {
            irqCounter = 0;
        }

        private void PokeE000(ushort address, ref byte data)
        {
            irqEnabled = false;
            Cpu.Irq(0);
        }

        private void PokeE001(ushort address, ref byte data)
        {
            irqEnabled = true;
        }

        protected override int DecodeChr(ushort address)
        {
            address ^= (ushort)(chrMode);

            switch (address & 0x1c00)
            {
            case 0x0000:
            case 0x0400: return (address & 0x7ff) | chrPages[0];
            case 0x0800:
            case 0x0c00: return (address & 0x7ff) | chrPages[1];
            case 0x1000: return (address & 0x3ff) | chrPages[2];
            case 0x1400: return (address & 0x3ff) | chrPages[3];
            case 0x1800: return (address & 0x3ff) | chrPages[4];
            case 0x1c00: return (address & 0x3ff) | chrPages[5];
            }

            throw new CompilerPleasingException();
        }

        protected override int DecodePrg(ushort address)
        {
            address ^= (ushort)(prgMode & ~(address << 1));

            switch (address & 0xe000)
            {
            case 0x8000: return (address & 0x1fff) | prgPages[0];
            case 0xa000: return (address & 0x1fff) | prgPages[1];
            case 0xc000: return (address & 0x1fff) | prgPages[2];
            case 0xe000: return (address & 0x1fff) | prgPages[3];
            }

            throw new CompilerPleasingException();
        }

        protected override void PeekRam(ushort address, ref byte data)
        {
            if (ramEnabled)
            {
                base.PeekRam(address, ref data);
            }
        }

        protected override void PokeRam(ushort address, ref byte data)
        {
            if (ramEnabled && !ramProtect)
                base.PokeRam(address, ref data);
        }

        public override void Consume(ClockSignal e)
        {
            // emulate phi2 filtering
            irqTimer++;
        }

        public override void Initialize()
        {
            base.Initialize();

            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = -2 << 13;
            prgPages[3] = -1 << 13;

            byte zero = 0;

            Poke8000(0x8000, ref zero);
            Poke8000(0x8001, ref zero);
            Poke8000(0xa000, ref zero);
            Poke8000(0xa001, ref zero);
            Poke8000(0xc000, ref zero);
            Poke8000(0xc001, ref zero);
            Poke8000(0xe000, ref zero);
            Poke8000(0xe001, ref zero);
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Decode("100- ---- ---- ---0").Poke(Poke8000);
            bus.Decode("100- ---- ---- ---1").Poke(Poke8001);
            bus.Decode("101- ---- ---- ---0").Poke(PokeA000);
            bus.Decode("101- ---- ---- ---1").Poke(PokeA001);
            bus.Decode("110- ---- ---- ---0").Poke(PokeC000);
            bus.Decode("110- ---- ---- ---1").Poke(PokeC001);
            bus.Decode("111- ---- ---- ---0").Poke(PokeE000);
            bus.Decode("111- ---- ---- ---1").Poke(PokeE001);
        }

        public override void PpuAddressUpdate(ushort address)
        {
            if (irqLatch < (address & 0x1000))
            { // rising edge
                if (irqTimer >= 5)
                {
                    if (irqCounter == 0)
                    {
                        irqCounter = irqRefresh;
                    }
                    else
                    {
                        irqCounter--;
                    }

                    if (irqCounter == 0 && irqEnabled)
                    {
                        Cpu.Irq(1);
                    }
                }

                irqTimer = 0;
            }

            irqLatch = (address & 0x1000);
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            }

            throw new CompilerPleasingException();
        }
    }
}
