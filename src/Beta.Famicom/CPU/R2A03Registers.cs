using Beta.Famicom.APU;
using Beta.Famicom.Input;

namespace Beta.Famicom.CPU
{
    public static class R2A03Registers
    {
        public static void read(R2A03State e, int address, ref byte data)
        {
            // switch (address & ~3)
            // {
            // case 0x4000: sq1.Read(address, ref data); break;
            // case 0x4004: sq2.Read(address, ref data); break;
            // case 0x4008: tri.Read(address, ref data); break;
            // case 0x400c: noi.Read(address, ref data); break;
            // case 0x4010: dmc.Read(address, ref data); break;
            // }

            if (address == 0x4014) { }

            if (address == 0x4015)
            {
                data = (byte)(
                    (e.sq1.duration.counter != 0 ? 0x01 : 0) |
                    (e.sq2.duration.counter != 0 ? 0x02 : 0) |
                    (e.tri.duration.counter != 0 ? 0x04 : 0) |
                    (e.noi.duration.counter != 0 ? 0x08 : 0) |
                    (e.sequence_irq_pending ? 0x40 : 0));

                e.sequence_irq_pending = false;
            }

            if (address == 0x4016)
            {
                data &= 0xe0;
                data |= InputConnector.readJoypad1();
            }

            if (address == 0x4017)
            {
                data &= 0xe0;
                data |= InputConnector.readJoypad2();
            }
        }

        public static void write(R2A03State e, int address, byte data)
        {
            switch (address & ~3)
            {
            case 0x4000: Sq1.write(e.sq1, address, data); break;
            case 0x4004: Sq2.write(e.sq2, address, data); break;
            case 0x4008: Tri.write(e.tri, address, data); break;
            case 0x400c: Noi.write(e.noi, address, data); break;
            case 0x4010: Dmc.write(e.dmc, address, data); break;
            }

            if (address == 0x4014)
            {
                e.dma_segment = data;
                e.dma_trigger = true;
            }

            if (address == 0x4015)
            {
                e.sq1.enabled = (data & 0x01) != 0;
                e.sq2.enabled = (data & 0x02) != 0;
                e.tri.enabled = (data & 0x04) != 0;
                e.noi.enabled = (data & 0x08) != 0;

                if (!e.sq1.enabled) { e.sq1.duration.counter = 0; }
                if (!e.sq2.enabled) { e.sq2.duration.counter = 0; }
                if (!e.tri.enabled) { e.tri.duration.counter = 0; }
                if (!e.noi.enabled) { e.noi.duration.counter = 0; }
            }

            if (address == 0x4016)
            {
                InputConnector.write(data);
            }

            if (address == 0x4017)
            {
                e.sequence_irq_enabled = (data & 0x40) == 0;

                if (e.sequence_irq_enabled == false)
                {
                    e.sequence_irq_pending = false;
                }

                e.sequence_mode = (data >> 7) & 1;
                e.sequence_time = 0;

                if (e.sequence_mode == 1)
                {
                    R2A03.halfFrameTick(e);
                    R2A03.quadFrameTick(e);
                }
            }
        }
    }
}
