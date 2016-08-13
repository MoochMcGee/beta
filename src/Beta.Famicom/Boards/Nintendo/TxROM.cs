using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards.Nintendo
{
    [BoardName("NES-T(.+)ROM")]
    public sealed class TxROM : IBoard, IConsumer<ClockSignal>
    {
        private readonly IProducer<IrqSignal> irq;

        private CartridgeImage image;

        private int chr_mode;
        private int nmt_mode;
        private int prg_mode;
        private int port_address;

        private int[] chr_page = new int[6];
        private int[] prg_page = new int[2];

        private bool irq_enabled;
        private int irq_address;
        private int irq_counter;
        private int irq_counter_latch;
        private int irq_timer;

        public TxROM(IProducer<IrqSignal> irq, ISignalBroker broker)
        {
            this.irq = irq;

            broker.Link(this);
        }

        public void ApplyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void R2A03Read(ushort address, ref byte data)
        {
            if ((address & 0x8000) != 0)
            {
                image.prg.Read(MapR2A03Address(address), ref data);
            }
        }

        public void R2A03Write(ushort address, byte data)
        {
            switch (address & 0xe001)
            {
            case 0x8000:
                chr_mode = (data << 5) & 0x1000;
                prg_mode = (data << 8) & 0x4000;
                port_address = (data >> 0) & 7;
                break;

            case 0x8001:
                switch (port_address)
                {
                case 0: chr_page[0] = data << 10; break;
                case 1: chr_page[1] = data << 10; break;
                case 2: chr_page[2] = data << 10; break;
                case 3: chr_page[3] = data << 10; break;
                case 4: chr_page[4] = data << 10; break;
                case 5: chr_page[5] = data << 10; break;
                case 6: prg_page[0] = data << 13; break;
                case 7: prg_page[1] = data << 13; break;
                }
                break;

            case 0xa000:
                nmt_mode = (data & 1);
                break;

            case 0xa001: break;

            case 0xc000:
                irq_counter_latch = data;
                break;

            case 0xc001:
                irq_counter = 0;
                break;

            case 0xe000:
                irq_enabled = false;
                irq.Produce(new IrqSignal(0));
                break;

            case 0xe001:
                irq_enabled = true;
                break;
            }
        }

        private int MapR2A03Address(ushort address)
        {
            address ^= (ushort)(prg_mode & ~(address << 1));

            switch (address & 0xe000)
            {
            case 0x8000: return (address & 0x1fff) | prg_page[0];
            case 0xa000: return (address & 0x1fff) | prg_page[1];
            case 0xc000: return (address & 0x1fff) | (~1 << 13);
            case 0xe000: return (address & 0x1fff) | (~0 << 13);
            }

            throw new CompilerPleasingException();
        }

        public void R2C02Read(ushort address, ref byte data)
        {
            ScanlineCounter(address);

            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Read(MapR2C02Address(address), ref data);
            }
        }

        public void R2C02Write(ushort address, byte data)
        {
            ScanlineCounter(address);

            if ((address & 0x2000) == 0x0000)
            {
                image.chr.Write(MapR2C02Address(address), data);
            }
        }

        private int MapR2C02Address(ushort address)
        {
            address ^= (ushort)(chr_mode);

            switch (address & 0x1c00)
            {
            case 0x0000:
            case 0x0400: return (address & 0x7ff) | (chr_page[0] & 0x3f800);
            case 0x0800:
            case 0x0c00: return (address & 0x7ff) | (chr_page[1] & 0x3f800);
            case 0x1000: return (address & 0x3ff) | (chr_page[2] & 0x3fc00);
            case 0x1400: return (address & 0x3ff) | (chr_page[3] & 0x3fc00);
            case 0x1800: return (address & 0x3ff) | (chr_page[4] & 0x3fc00);
            case 0x1c00: return (address & 0x3ff) | (chr_page[5] & 0x3fc00);
            }

            throw new CompilerPleasingException();
        }

        private void ScanlineCounter(ushort address)
        {
            if ((irq_address & 0x1000) < (address & 0x1000))
            {
                if (irq_timer > 6)
                {
                    if (irq_counter == 0)
                    {
                        irq_counter = irq_counter_latch;
                    }
                    else
                    {
                        irq_counter--;

                        if (irq_counter == 0 && irq_enabled)
                        {
                            irq.Produce(new IrqSignal(1));
                        }
                    }
                }

                irq_timer = 0;
            }

            irq_address = address;
        }

        public bool VRAM(ushort address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (nmt_mode)
            {
            case 0: a10 = x; return true;
            case 1: a10 = y; return true;
            }

            throw new CompilerPleasingException();
        }

        public void Consume(ClockSignal e)
        {
            irq_timer++;
        }
    }
}
