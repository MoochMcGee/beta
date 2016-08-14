using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02Registers
    {
        private readonly CGRAM cgram;
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State r2c02;
        private readonly IProducer<VblSignal> vbl;

        public R2C02Registers(CGRAM cgram, State state, R2C02MemoryMap memory, IProducer<VblSignal> vbl)
        {
            this.cgram = cgram;
            this.r2c02 = state.r2c02;
            this.memory = memory;
            this.vbl = vbl;
        }

        public void Read(ushort address, ref byte data)
        {
            switch (address & 0x2007)
            {
            case 0x2000: break;
            case 0x2001: break;
            case 0x2002: Read2002(ref data); break;
            case 0x2003: break;
            case 0x2004: Read2004(ref data); break;
            case 0x2005: break;
            case 0x2006: break;
            case 0x2007: Read2007(ref data); break;
            }
        }

        private void Read2002(ref byte data)
        {
            data &= 0x1f;

            if (r2c02.vbl_flag > 0) data |= 0x80;
            if (r2c02.obj_zero_hit) data |= 0x40;
            if (r2c02.obj_overflow) data |= 0x20;

            r2c02.vbl_hold = 0;
            r2c02.vbl_flag = 0;
            r2c02.scroll_swap = false;

            VBL();
        }

        private void Read2004(ref byte data)
        {
            data = r2c02.oam[r2c02.oam_address];
        }

        private void Read2007(ref byte data)
        {
            if ((r2c02.scroll_address & 0x3f00) == 0x3f00)
            {
                data = cgram.Read(r2c02.scroll_address);
            }
            else
            {
                data = r2c02.chr;
            }

            memory.Read(r2c02.scroll_address, ref r2c02.chr);

            r2c02.scroll_address += r2c02.scroll_step;
            r2c02.scroll_address &= 0x7fff;
        }

        public void Write(ushort address, byte data)
        {
            switch (address & 0x2007)
            {
            case 0x2000: Write2000(data); break;
            case 0x2001: Write2001(data); break;
            case 0x2002: break;
            case 0x2003: Write2003(data); break;
            case 0x2004: Write2004(data); break;
            case 0x2005: Write2005(data); break;
            case 0x2006: Write2006(data); break;
            case 0x2007: Write2007(data); break;
            }
        }

        private void Write2000(byte data)
        {
            r2c02.scroll_temp = (ushort)((r2c02.scroll_temp & 0x73ff) | ((data << 10) & 0x0c00));
            r2c02.scroll_step = (ushort)((data & 0x04) != 0 ? 0x0020 : 0x0001);
            r2c02.obj_address = (ushort)((data & 0x08) != 0 ? 0x1000 : 0x0000);
            r2c02.bkg_address = (ushort)((data & 0x10) != 0 ? 0x1000 : 0x0000);
            r2c02.obj_rasters = (data & 0x20) != 0 ? 0x0010 : 0x0008;
            r2c02.vbl_enabled = (data & 0x80) >> 7;

            VBL();
        }

        private void Write2001(byte data)
        {
            r2c02.bkg_clipped = (data & 0x02) == 0;
            r2c02.obj_clipped = (data & 0x04) == 0;
            r2c02.bkg_enabled = (data & 0x08) != 0;
            r2c02.obj_enabled = (data & 0x10) != 0;

            r2c02.clipping = (data & 0x01) != 0 ? 0x30 : 0x3f;
            r2c02.emphasis = (data & 0xe0) << 1;
        }

        private void Write2003(byte data)
        {
            r2c02.oam_address = data;
        }

        private void Write2004(byte data)
        {
            if ((r2c02.oam_address & 3) == 2)
                data &= 0xe3;

            r2c02.oam[r2c02.oam_address++] = data;
        }

        private void Write2005(byte data)
        {
            if (r2c02.scroll_swap = !r2c02.scroll_swap)
            {
                r2c02.scroll_temp = (ushort)((r2c02.scroll_temp & ~0x001f) | ((data & ~7) >> 3));
                r2c02.scroll_fine = (data & 0x07);
            }
            else
            {
                r2c02.scroll_temp = (ushort)((r2c02.scroll_temp & ~0x73e0) | ((data & 7) << 12) | ((data & ~7) << 2));
            }
        }

        private void Write2006(byte data)
        {
            if (r2c02.scroll_swap = !r2c02.scroll_swap)
            {
                r2c02.scroll_temp = (ushort)((r2c02.scroll_temp & ~0xff00) | ((data & 0x3f) << 8));
            }
            else
            {
                r2c02.scroll_temp = (ushort)((r2c02.scroll_temp & ~0x00ff) | ((data & 0xff) << 0));
                r2c02.scroll_address = r2c02.scroll_temp;

                memory.Read(r2c02.scroll_address, ref data);
            }
        }

        private void Write2007(byte data)
        {
            if ((r2c02.scroll_address & 0x3f00) == 0x3f00)
            {
                cgram.Write(r2c02.scroll_address, data);
            }
            else
            {
                memory.Write(r2c02.scroll_address, data);
            }

            r2c02.scroll_address += r2c02.scroll_step;
            r2c02.scroll_address &= 0x7fff;
        }

        private void VBL()
        {
            var signal = r2c02.vbl_enabled & r2c02.vbl_flag;

            vbl.Produce(new VblSignal(signal));
        }
    }
}
