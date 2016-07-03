namespace Beta.GameBoy.Memory
{
    public sealed class MMIO : IMemory
    {
        private readonly Registers regs;

        public MMIO(Registers regs)
        {
            this.regs = regs;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
            case 0xff00:
                if (regs.pad.p15) return regs.pad.p15_latch;
                if (regs.pad.p14) return regs.pad.p14_latch;
                return 0xff;

            case 0xff04: return regs.tma.divider;
            case 0xff05: return regs.tma.counter;
            case 0xff06: return regs.tma.modulus;
            case 0xff07: return regs.tma.control;

            case 0xff0f: return regs.cpu.irf;

            case 0xff40: return regs.ppu.ff40;
            case 0xff41: return (byte)(0x80 | regs.ppu.control);
            case 0xff42: return regs.ppu.scroll_y;
            case 0xff43: return regs.ppu.scroll_x;
            case 0xff44: return regs.ppu.v;
            case 0xff45: return regs.ppu.v_check;
            case 0xff46: return regs.ppu.dma_segment;
            case 0xff47: return regs.ppu.bkg_palette;
            case 0xff48: return regs.ppu.obj_palette[0];
            case 0xff49: return regs.ppu.obj_palette[1];
            case 0xff4a: return regs.ppu.window_y;
            case 0xff4b: return regs.ppu.window_x;

            case 0xffff: return regs.cpu.ief;
            }

            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            switch (address)
            {
            case 0xff00:
                regs.pad.p15 = (data & 0x20) == 0;
                regs.pad.p14 = (data & 0x10) == 0;
                break;

            case 0xff04: regs.tma.divider = 0x00; break;
            case 0xff05: regs.tma.counter = data; break;
            case 0xff06: regs.tma.modulus = data; break;
            case 0xff07: regs.tma.control = data; break;

            case 0xff0f: regs.cpu.irf = data; break;

            case 0xff40:
                if (!regs.ppu.lcd_enabled && (data & 0x80) != 0)
                {
                    // lcd turning on
                    regs.ppu.h = 4;
                    regs.ppu.v = 0;
                }

                regs.ppu.ff40 = data;

                regs.ppu.lcd_enabled = (data & 0x80) != 0; // Bit 7 - LCD Display Enable - (0=Off, 1=On)
                regs.ppu.wnd_name_address = (data & 0x40) != 0 ? 0x9C00 : 0x9800; // Bit 6 - Wnd Tile Map Display Select    (0=9800-9BFF, 1=9C00-9FFF)
                regs.ppu.wnd_enabled = (data & 0x20) != 0; // Bit 5 - Wnd Display Enable - (0=Off, 1=On)
                regs.ppu.bkg_char_address = (data & 0x10) != 0 ? 0x8000 : 0x9000; // Bit 4 - Bkg & Wnd Tile Data Select     (0=8800-97FF, 1=8000-8FFF)
                regs.ppu.bkg_name_address = (data & 0x08) != 0 ? 0x9C00 : 0x9800; // Bit 3 - Bkg Tile Map Display Select    (0=9800-9BFF, 1=9C00-9FFF)
                regs.ppu.obj_rasters = (data & 0x04) != 0 ? 16 : 8;
                regs.ppu.obj_enabled = (data & 0x02) != 0; // Bit 1 - Spr Display Enable - (0=Off, 1=On)
                regs.ppu.bkg_enabled = (data & 0x01) != 0; // Bit 0 - Bkg Display Enable - (0=Off, 1=On)
                break;

            case 0xff41:
                regs.ppu.control &= ~0x87;
                regs.ppu.control |= (data & 0x78);
                break;

            case 0xff42: regs.ppu.scroll_y = data; break;
            case 0xff43: regs.ppu.scroll_x = data; break;
            case 0xff44:
                regs.ppu.h = 0;
                regs.ppu.v = 0;
                break;

            case 0xff45: regs.ppu.v_check = data; break;
            case 0xff46:
                regs.ppu.dma_triggered = true;
                regs.ppu.dma_segment = data;
                break;

            case 0xff47: regs.ppu.bkg_palette = data; break;
            case 0xff48: regs.ppu.obj_palette[0] = data; break;
            case 0xff49: regs.ppu.obj_palette[1] = data; break;
            case 0xff4a: regs.ppu.window_y = data; break;
            case 0xff4b: regs.ppu.window_x = data; break;

            case 0xff50: regs.boot_rom_enabled = false; break;

            case 0xffff: regs.cpu.ief = data; break;
            }
        }
    }
}
