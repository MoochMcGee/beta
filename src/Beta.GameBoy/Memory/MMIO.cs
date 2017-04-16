using Beta.GameBoy.APU;

namespace Beta.GameBoy.Memory
{
    public sealed class MMIO
    {
        private readonly State state;
        private readonly HRAM hram;
        private readonly Wave wave;

        public MMIO(State state, HRAM hram, Wave wave)
        {
            this.state = state;
            this.hram = hram;
            this.wave = wave;
        }

        public byte Read(ushort address)
        {
            if (address >= 0xff10 && address <= 0xff2f)
            {
                return ApuRegisters.Read(state.apu, address);
            }
            else if (address >= 0xff30 && address <= 0xff3f)
            {
                return wave.Read(address);
            }
            else if (address >= 0xff80 && address <= 0xfffe)
            {
                return hram.Read(address);
            }

            switch (address)
            {
            case 0xff00:
                if (state.pad.p15) return state.pad.p15_latch;
                if (state.pad.p14) return state.pad.p14_latch;
                return 0xff;

            case 0xff04: return Tma.getDivider(state.tma);
            case 0xff05: return Tma.getCounter(state.tma);
            case 0xff06: return Tma.getModulus(state.tma);
            case 0xff07: return Tma.getControl(state.tma);

            case 0xff0f: return state.cpu.irf;

            case 0xff40: return state.ppu.ff40;
            case 0xff41: return (byte)(0x80 | state.ppu.control);
            case 0xff42: return state.ppu.scroll_y;
            case 0xff43: return state.ppu.scroll_x;
            case 0xff44: return state.ppu.v;
            case 0xff45: return state.ppu.v_check;
            case 0xff46: return state.ppu.dma_segment;
            case 0xff47: return state.ppu.bkg_palette;
            case 0xff48: return state.ppu.obj_palette[0];
            case 0xff49: return state.ppu.obj_palette[1];
            case 0xff4a: return state.ppu.window_y;
            case 0xff4b: return state.ppu.window_x;

            case 0xffff: return state.cpu.ief;
            }

            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            if (address >= 0xff10 && address <= 0xff2f)
            {
                ApuRegisters.Write(state.apu, address, data);
            }
            else if (address >= 0xff30 && address <= 0xff3f)
            {
                wave.Write(address, data);
            }
            else if (address >= 0xff80 && address <= 0xfffe)
            {
                hram.Write(address, data);
            }
            else
            {
                switch (address)
                {
                case 0xff00:
                    state.pad.p15 = (data & 0x20) == 0;
                    state.pad.p14 = (data & 0x10) == 0;
                    break;

                case 0xff04: Tma.setDivider(state.tma, data); break;
                case 0xff05: Tma.setCounter(state.tma, data); break;
                case 0xff06: Tma.setModulus(state.tma, data); break;
                case 0xff07: Tma.setControl(state.tma, data); break;

                case 0xff0f: state.cpu.irf = data; break;

                // ppu
                case 0xff40:
                    if (!state.ppu.lcd_enabled && (data & 0x80) != 0)
                    {
                        // lcd turning on
                        state.ppu.h = 4;
                        state.ppu.v = 0;
                    }

                    state.ppu.ff40 = data;

                    state.ppu.lcd_enabled      = (data & 0x80) != 0;
                    state.ppu.wnd_name_address = (data & 0x40) != 0 ? 0x9C00 : 0x9800;
                    state.ppu.wnd_enabled      = (data & 0x20) != 0;
                    state.ppu.bkg_char_address = (data & 0x10) != 0 ? 0x8000 : 0x9000;
                    state.ppu.bkg_name_address = (data & 0x08) != 0 ? 0x9C00 : 0x9800;
                    state.ppu.obj_rasters      = (data & 0x04) != 0 ? 16 : 8;
                    state.ppu.obj_enabled      = (data & 0x02) != 0;
                    state.ppu.bkg_enabled      = (data & 0x01) != 0;
                    break;

                case 0xff41:
                    state.ppu.control &= ~0x87;
                    state.ppu.control |= (data & 0x78);
                    break;

                case 0xff42: state.ppu.scroll_y = data; break;
                case 0xff43: state.ppu.scroll_x = data; break;
                case 0xff44:
                    state.ppu.h = 0;
                    state.ppu.v = 0;
                    break;

                case 0xff45: state.ppu.v_check = data; break;
                case 0xff46:
                    state.ppu.dma_trigger = true;
                    state.ppu.dma_segment = data;
                    break;

                case 0xff47: state.ppu.bkg_palette = data; break;
                case 0xff48: state.ppu.obj_palette[0] = data; break;
                case 0xff49: state.ppu.obj_palette[1] = data; break;
                case 0xff4a: state.ppu.window_y = data; break;
                case 0xff4b: state.ppu.window_x = data; break;

                case 0xff50: state.boot_rom_enabled = false; break;

                case 0xffff: state.cpu.ief = data; break;
                }
            }
        }
    }
}
