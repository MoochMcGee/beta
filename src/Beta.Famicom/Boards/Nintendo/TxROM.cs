using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards.Nintendo
{
    public sealed class TxROM : IBoard
    {
        private readonly IProducer<IrqSignal> irq;

        private CartridgeImage image;

        private int chr_mode;
        private int nmt_mode;
        private int prg_mode;
        private int reg_address;

        private int[] chr_page = new int[8];
        private int[] prg_page = new int[4];

        private bool irq_enabled;
        private int irq_address;
        private int irq_counter;
        private int irq_counter_latch;
        private int irq_timer;

        public TxROM(IProducer<IrqSignal> irq, SignalBroker broker)
        {
            this.irq = irq;

            prg_page[2] = ~1 << 13;
            prg_page[3] = ~0 << 13;

            broker.Link<ClockSignal>(this.consume);
        }

        public void applyImage(CartridgeImage image)
        {
            this.image = image;
        }

        public void r2a03Read(int address, ref byte data)
        {
            if ((address & 0xe000) == 0x6000)
            {
                image.wram.read(address, ref data);
            }

            if ((address & 0x8000) == 0x8000)
            {
                image.prg.read(mapR2A03Address(address), ref data);
            }
        }

        public void r2a03Write(int address, byte data)
        {
            if ((address & 0xe000) == 0x6000)
            {
                image.wram.write(address, data);
            }

            switch (address & 0xe001)
            {
            case 0x8000:
                chr_mode = (data << 5) & 0x1000;
                prg_mode = (data << 8) & 0x4000;
                reg_address = (data >> 0) & 7;
                break;

            case 0x8001:
                switch (reg_address)
                {
                case 0:
                    chr_page[0] = ((data << 10) & 0x3f800) | 0x000;
                    chr_page[1] = ((data << 10) & 0x3f800) | 0x400;
                    break;

                case 1:
                    chr_page[2] = ((data << 10) & 0x3f800) | 0x000;
                    chr_page[3] = ((data << 10) & 0x3f800) | 0x400;
                    break;

                case 2: chr_page[4] = data << 10; break;
                case 3: chr_page[5] = data << 10; break;
                case 4: chr_page[6] = data << 10; break;
                case 5: chr_page[7] = data << 10; break;
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

        private int mapR2A03Address(int address)
        {
            address ^= prg_mode & ~(address << 1);

            int page = (address >> 13) & 3;

            return (address & 0x1fff) | (prg_page[page] & 0x1fe000);
        }

        public void r2c02Read(int address, ref byte data)
        {
            scanlineCounter(address);

            if ((address & 0x2000) == 0x0000)
            {
                image.chr.read(mapR2C02Address(address), ref data);
            }
        }

        public void r2c02Write(int address, byte data)
        {
            scanlineCounter(address);

            if ((address & 0x2000) == 0x0000)
            {
                image.chr.write(mapR2C02Address(address), data);
            }
        }

        private int mapR2C02Address(int address)
        {
            address ^= chr_mode;

            int page = (address >> 10) & 7;

            return (address & 0x3ff) | (chr_page[page] & 0x3fc00);
        }

        private void scanlineCounter(int address)
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

        public bool vram(int address, out int a10)
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

        public void consume(ClockSignal e)
        {
            irq_timer++;
        }
    }
}
