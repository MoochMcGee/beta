namespace Beta.GameBoy.Memory
{
    public sealed class MMIO
    {
        private readonly Registers regs;
        private readonly HRAM hram;
        private readonly Wave wave;

        public MMIO(Registers regs, HRAM hram, Wave wave)
        {
            this.regs = regs;
            this.hram = hram;
            this.wave = wave;
        }

        public byte Read(ushort address)
        {
            if (address >= 0xff30 && address <= 0xff3f)
            {
                return wave.Read(address);
            }

            if (address >= 0xff80 && address <= 0xfffe)
            {
                return hram.Read(address);
            }

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

            case 0xff26:
                return (byte)(
                    (regs.noi.enabled ? 0x08 : 0) |
                    (regs.wav.enabled ? 0x04 : 0) |
                    (regs.sq2.enabled ? 0x02 : 0) |
                    (regs.sq1.enabled ? 0x01 : 0)
                );

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
            if (address >= 0xff30 && address <= 0xff3f)
            {
                wave.Write(address, data);
                return;
            }

            if (address >= 0xff80 && address <= 0xfffe)
            {
                hram.Write(address, data);
                return;
            }

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

            // apu::sq1

            case 0xff10:
                regs.sq1.sweep_period = (data >> 4) & 7;
                regs.sq1.sweep_direction = (data >> 3) & 1;
                regs.sq1.sweep_shift = (data >> 0) & 7;
                break;

            case 0xff11:
                regs.sq1.duty_form = (data >> 6) & 3;
                regs.sq1.duration_latch = (data >> 0) & 63;
                break;

            case 0xff12:
                regs.sq1.volume = (data >> 4) & 15;
                regs.sq1.volume_direction = (data >> 3) & 1;
                regs.sq1.volume_period = (data >> 0) & 7;
                break;

            case 0xff13:
                regs.sq1.period = (regs.sq1.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff14:
                regs.sq1.period = (regs.sq1.period & 0x0ff) | ((data << 8) & 0x700);
                regs.sq1.duration_loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0)
                {
                    regs.sq1.timer = 2048 - regs.sq1.period;
                    regs.sq1.duration = 64 - regs.sq1.duration_latch;
                    regs.sq1.volume_timer = regs.sq1.volume_period;
                    regs.sq1.enabled = true;

                    regs.sq1.sweep_timer = regs.sq1.sweep_period;
                    regs.sq1.sweep_enabled = regs.sq1.sweep_period != 0 && regs.sq1.sweep_shift != 0;
                }
                break;

            // apu::sq2

            case 0xff16:
                regs.sq2.duty_form = (data >> 6) & 3;
                regs.sq2.duration_latch = (data >> 0) & 63;
                break;

            case 0xff17:
                regs.sq2.volume = (data >> 4) & 15;
                regs.sq2.volume_direction = (data >> 3) & 1;
                regs.sq2.volume_period = (data >> 0) & 7;
                break;

            case 0xff18:
                regs.sq2.period = (regs.sq2.period & 0x700) | ((data << 0) & 0x0ff);
                break;

            case 0xff19:
                regs.sq2.period = (regs.sq2.period & 0x0ff) | ((data << 8) & 0x700);
                regs.sq2.duration_loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0)
                {
                    regs.sq2.timer = 2048 - regs.sq2.period;
                    regs.sq2.duration = 64 - regs.sq2.duration_latch;
                    regs.sq2.volume_timer = regs.sq2.volume_period;
                    regs.sq2.enabled = true;
                }
                break;

            // apu::wav

            case 0xff1a: break;
            case 0xff1b:
                regs.wav.duration_latch = data;
                break;

            case 0xff1c:
                switch ((data >> 5) & 3)
                {
                case 0: regs.wav.volume_shift = 4; break;
                case 1: regs.wav.volume_shift = 0; break;
                case 2: regs.wav.volume_shift = 1; break;
                case 3: regs.wav.volume_shift = 2; break;
                }
                break;

            case 0xff1d:
                regs.wav.period = (regs.wav.period & 0x700) | ((data << 0) & 0xff);
                break;

            case 0xff1e:
                regs.wav.period = (regs.wav.period & 0x0ff) | ((data << 8) & 0x700);
                regs.wav.duration_loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0)
                {
                    regs.wav.timer = (2048 - regs.wav.period) / 2;
                    regs.wav.duration = 256 - regs.wav.duration_latch;
                    regs.wav.wave_ram_cursor = 0;
                    regs.wav.wave_ram_shift = 4;
                    regs.wav.enabled = true;
                }
                break;

            // apu::noi

            case 0xff20:
                regs.noi.duration_latch = (data >> 0) & 63;
                break;

            case 0xff21:
                regs.noi.volume_latch = (data >> 4) & 15;
                regs.noi.volume_direction = (data >> 3) & 1;
                regs.noi.volume_period = (data >> 0) & 7;
                break;

            case 0xff22:
                switch (data & 7)
                {
                case 0: regs.noi.period = (0x08 << (data >> 4)) / 4; break;
                case 1: regs.noi.period = (0x10 << (data >> 4)) / 4; break;
                case 2: regs.noi.period = (0x20 << (data >> 4)) / 4; break;
                case 3: regs.noi.period = (0x30 << (data >> 4)) / 4; break;
                case 4: regs.noi.period = (0x40 << (data >> 4)) / 4; break;
                case 5: regs.noi.period = (0x50 << (data >> 4)) / 4; break;
                case 6: regs.noi.period = (0x60 << (data >> 4)) / 4; break;
                case 7: regs.noi.period = (0x70 << (data >> 4)) / 4; break;
                }

                regs.noi.lfsr_mode = (data >> 3) & 1;
                break;

            case 0xff23:
                regs.noi.duration_loop = (data & 0x40) == 0;

                if ((data & 0x80) != 0)
                {
                    regs.noi.timer = regs.noi.period;
                    regs.noi.duration = 64 - regs.noi.duration_latch;
                    regs.noi.volume = regs.noi.volume_latch;
                    regs.noi.volume_timer = regs.noi.volume_period;
                    regs.noi.lfsr = 0x7fff;
                    regs.noi.enabled = true;
                }
                break;

            // apu

            case 0xff24:
                regs.apu.speaker_volume[0] = (byte)((data >> 4) & 7);
                regs.apu.speaker_volume[1] = (byte)((data >> 0) & 7);
                break;

            case 0xff25:
                regs.apu.speaker_select[0] = (byte)((data >> 4) & 15);
                regs.apu.speaker_select[1] = (byte)((data >> 0) & 15);
                break;

            case 0xff26:
                break;

            // ppu

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
