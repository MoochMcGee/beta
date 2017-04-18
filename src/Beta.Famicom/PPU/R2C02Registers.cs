using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.PPU
{
    public static class R2C02Registers
    {
        public static void Read(R2C02State e, int address, ref byte data)
        {
            switch (address & 0x2007)
            {
            case 0x2000: break;
            case 0x2001: break;
            case 0x2002: Read2002(e, ref data); break;
            case 0x2003: break;
            case 0x2004: Read2004(e, ref data); break;
            case 0x2005: break;
            case 0x2006: break;
            case 0x2007: Read2007(e, ref data); break;
            }
        }

        static void Read2002(R2C02State e, ref byte data)
        {
            data &= 0x1f;

            if (e.vbl_flag > 0) data |= 0x80;
            if (e.obj_zero_hit) data |= 0x40;
            if (e.obj_overflow) data |= 0x20;

            e.vbl_hold = 0;
            e.vbl_flag = 0;
            e.scroll_swap = false;
        }

        static void Read2004(R2C02State e, ref byte data)
        {
            data = e.oam[e.oam_address];
        }

        static void Read2007(R2C02State e, ref byte data)
        {
            if ((e.scroll_address & 0x3f00) == 0x3f00)
            {
                data = CGRAM.Read(e, e.scroll_address);
            }
            else
            {
                data = e.chr;
            }

            R2C02MemoryMap.Read(e.scroll_address, ref e.chr);

            e.scroll_address += e.scroll_step;
            e.scroll_address &= 0x7fff;
        }

        public static void Write(R2C02State e, int address, byte data)
        {
            switch (address & 0x2007)
            {
            case 0x2000: Write2000(e, data); break;
            case 0x2001: Write2001(e, data); break;
            case 0x2002: break;
            case 0x2003: Write2003(e, data); break;
            case 0x2004: Write2004(e, data); break;
            case 0x2005: Write2005(e, data); break;
            case 0x2006: Write2006(e, data); break;
            case 0x2007: Write2007(e, data); break;
            }
        }

        static void Write2000(R2C02State e, byte data)
        {
            e.scroll_temp = (e.scroll_temp & 0x73ff) | ((data << 10) & 0x0c00);
            e.scroll_step = (data & 0x04) != 0 ? 0x0020 : 0x0001;
            e.obj_address = (data & 0x08) != 0 ? 0x1000 : 0x0000;
            e.bkg_address = (data & 0x10) != 0 ? 0x1000 : 0x0000;
            e.obj_rasters = (data & 0x20) != 0 ? 0x0010 : 0x0008;
            e.vbl_enabled = (data & 0x80) >> 7;
        }

        static void Write2001(R2C02State e, byte data)
        {
            e.bkg_clipped = (data & 0x02) == 0;
            e.obj_clipped = (data & 0x04) == 0;
            e.bkg_enabled = (data & 0x08) != 0;
            e.obj_enabled = (data & 0x10) != 0;

            e.clipping = (data & 0x01) != 0 ? 0x30 : 0x3f;
            e.emphasis = (data & 0xe0) << 1;
        }

        static void Write2003(R2C02State e, byte data)
        {
            e.oam_address = data;
        }

        static void Write2004(R2C02State e, byte data)
        {
            if ((e.oam_address & 3) == 2)
                data &= 0xe3;

            e.oam[e.oam_address++] = data;
        }

        static void Write2005(R2C02State e, byte data)
        {
            if (e.scroll_swap = !e.scroll_swap)
            {
                e.scroll_temp = (e.scroll_temp & ~0x001f) | ((data & ~7) >> 3);
                e.scroll_fine = (data & 0x07);
            }
            else
            {
                e.scroll_temp = (e.scroll_temp & ~0x73e0) | ((data & 7) << 12) | ((data & ~7) << 2);
            }
        }

        static void Write2006(R2C02State e, byte data)
        {
            if (e.scroll_swap = !e.scroll_swap)
            {
                e.scroll_temp = (e.scroll_temp & ~0xff00) | ((data & 0x3f) << 8);
            }
            else
            {
                e.scroll_temp = (e.scroll_temp & ~0x00ff) | ((data & 0xff) << 0);
                e.scroll_address = e.scroll_temp;

                R2C02MemoryMap.Read(e.scroll_address, ref data);
            }
        }

        static void Write2007(R2C02State e, byte data)
        {
            if ((e.scroll_address & 0x3f00) == 0x3f00)
            {
                CGRAM.Write(e, e.scroll_address, data);
            }
            else
            {
                R2C02MemoryMap.Write(e.scroll_address, data);
            }

            e.scroll_address += e.scroll_step;
            e.scroll_address &= 0x7fff;
        }
    }
}
