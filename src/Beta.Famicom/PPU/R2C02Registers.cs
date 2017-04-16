using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02Registers
    {
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State state;
        private readonly IProducer<VblSignal> vbl;

        public R2C02Registers(State state, R2C02MemoryMap memory, IProducer<VblSignal> vbl)
        {
            this.state = state.r2c02;
            this.memory = memory;
            this.vbl = vbl;
        }

        public void Read(int address, ref byte data)
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

            if (state.vbl_flag > 0) data |= 0x80;
            if (state.obj_zero_hit) data |= 0x40;
            if (state.obj_overflow) data |= 0x20;

            state.vbl_hold = 0;
            state.vbl_flag = 0;
            state.scroll_swap = false;

            VBL();
        }

        private void Read2004(ref byte data)
        {
            data = state.oam[state.oam_address];
        }

        private void Read2007(ref byte data)
        {
            if ((state.scroll_address & 0x3f00) == 0x3f00)
            {
                data = CGRAM.Read(state, state.scroll_address);
            }
            else
            {
                data = state.chr;
            }

            memory.Read(state.scroll_address, ref state.chr);

            state.scroll_address += state.scroll_step;
            state.scroll_address &= 0x7fff;
        }

        public void Write(int address, byte data)
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
            state.scroll_temp = (state.scroll_temp & 0x73ff) | ((data << 10) & 0x0c00);
            state.scroll_step = (data & 0x04) != 0 ? 0x0020 : 0x0001;
            state.obj_address = (data & 0x08) != 0 ? 0x1000 : 0x0000;
            state.bkg_address = (data & 0x10) != 0 ? 0x1000 : 0x0000;
            state.obj_rasters = (data & 0x20) != 0 ? 0x0010 : 0x0008;
            state.vbl_enabled = (data & 0x80) >> 7;

            VBL();
        }

        private void Write2001(byte data)
        {
            state.bkg_clipped = (data & 0x02) == 0;
            state.obj_clipped = (data & 0x04) == 0;
            state.bkg_enabled = (data & 0x08) != 0;
            state.obj_enabled = (data & 0x10) != 0;

            state.clipping = (data & 0x01) != 0 ? 0x30 : 0x3f;
            state.emphasis = (data & 0xe0) << 1;
        }

        private void Write2003(byte data)
        {
            state.oam_address = data;
        }

        private void Write2004(byte data)
        {
            if ((state.oam_address & 3) == 2)
                data &= 0xe3;

            state.oam[state.oam_address++] = data;
        }

        private void Write2005(byte data)
        {
            if (state.scroll_swap = !state.scroll_swap)
            {
                state.scroll_temp = (state.scroll_temp & ~0x001f) | ((data & ~7) >> 3);
                state.scroll_fine = (data & 0x07);
            }
            else
            {
                state.scroll_temp = (state.scroll_temp & ~0x73e0) | ((data & 7) << 12) | ((data & ~7) << 2);
            }
        }

        private void Write2006(byte data)
        {
            if (state.scroll_swap = !state.scroll_swap)
            {
                state.scroll_temp = (state.scroll_temp & ~0xff00) | ((data & 0x3f) << 8);
            }
            else
            {
                state.scroll_temp = (state.scroll_temp & ~0x00ff) | ((data & 0xff) << 0);
                state.scroll_address = state.scroll_temp;

                memory.Read(state.scroll_address, ref data);
            }
        }

        private void Write2007(byte data)
        {
            if ((state.scroll_address & 0x3f00) == 0x3f00)
            {
                CGRAM.Write(state, state.scroll_address, data);
            }
            else
            {
                memory.Write(state.scroll_address, data);
            }

            state.scroll_address += state.scroll_step;
            state.scroll_address &= 0x7fff;
        }

        private void VBL()
        {
            var signal = state.vbl_enabled & state.vbl_flag;

            vbl.Produce(new VblSignal(signal));
        }
    }
}
