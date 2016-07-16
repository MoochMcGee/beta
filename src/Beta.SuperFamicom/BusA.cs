using System;
using Beta.SuperFamicom.Cartridges;
using Beta.SuperFamicom.CPU;
using Beta.SuperFamicom.Memory;

namespace Beta.SuperFamicom
{
    public sealed class BusA
    {
        private readonly State state;
        private readonly WRAM wram;
        private readonly BusB busB;

        private ICartridge cart;

        public Cpu Cpu;

        public BusA(State state, WRAM wram, BusB busB)
        {
            this.state = state;
            this.wram = wram;
            this.busB = busB;
        }

        public void Initialize(ICartridge cart)
        {
            this.cart = cart;
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            if ((bank & 0x7e) == 0x7e) { wram.Read(bank, address, ref data); return; }
            if ((bank & 0x7f) <= 0x3f)
            {
                if ((address & 0xe000) == 0x0000) { wram.Read(0x00, address, ref data); return; }
                if ((address & 0xff00) == 0x2100) { busB.Read(bank, address, ref data); return; }
                if ((address & 0xfc00) == 0x4000) { ReadSCPU(bank, address, ref data); return; }
                if ((address & 0x8000) == 0x8000) { cart.Read(bank, address, ref data); return; }
                return;
            }

            if ((bank & 0x7f) <= 0x7f) { cart.Read(bank, address, ref data); return; }

            throw new NotImplementedException();
        }

        private void ReadSCPU(byte bank, ushort address, ref byte data)
        {
            switch (address)
            {
            case 0x4016: break; // JOYSER0
            case 0x4017: break; // JOYSER1

            case 0x4210: data = (byte)((data & 0x70) | (state.scpu.in_vblank ? 0x80 : 0) | 0x02); break;
            case 0x4211: data = (byte)((data & 0x7f) | (state.scpu.timer_coincidence ? 0x80 : 0)); break;
            case 0x4212: data = (byte)((data & 0x3e) | (state.scpu.in_vblank ? 0x80 : 0) | (state.scpu.in_hblank ? 0x40 : 0)); break;
            case 0x4213: break; // I/O Port
            case 0x4214: data = (byte)(state.scpu.rddiv >> 0); break; // RDDIVL
            case 0x4215: data = (byte)(state.scpu.rddiv >> 8); break; // RDDIVH
            case 0x4216: data = (byte)(state.scpu.rdmpy >> 0); break; // RDMPYL
            case 0x4217: data = (byte)(state.scpu.rdmpy >> 8); break; // RDMPYH

            case 0x4218: data = (byte)(state.pads[0] >> 0); break;
            case 0x4219: data = (byte)(state.pads[0] >> 8); break;

            case 0x421a: data = (byte)(state.pads[1] >> 0); break;
            case 0x421b: data = (byte)(state.pads[1] >> 8); break;

            case 0x421c: data = 0; break; // JOY3L
            case 0x421d: data = 0; break; // JOY3H

            case 0x421e: data = 0; break; // JOY4L
            case 0x421f: data = 0; break; // JOY4H
            }

            // throw new NotImplementedException($"Unknown address: ${bank:x2}:{address:x4}.");
        }

        public void Write(byte bank, ushort address, byte data)
        {
            if ((bank & 0x7e) == 0x7e) { wram.Write(bank, address, data); return; }
            if ((bank & 0x7f) <= 0x3f)
            {
                if ((address & 0xe000) == 0x0000) { wram.Write(0x00, address, data); return; } // $0000-$1fff
                if ((address & 0xff00) == 0x2100) { busB.Write(bank, address, data); return; } // $2100-$21ff
                if ((address & 0xfc00) == 0x4000) { WriteSCPU(bank, address, data); return; } // $4000-$43ff
                if ((address & 0x8000) == 0x8000) { cart.Write(bank, address, data); return; } // $8000-$ffff
                return;
            }

            if ((bank & 0x7f) <= 0x7f) { cart.Write(bank, address, data); return; }

            throw new NotImplementedException();
        }

        private void WriteSCPU(byte bank, ushort address, byte data)
        {
            if ((address & 0xff00) == 0x4300)
            {
                var dma = state.scpu.dma[(address >> 4) & 7];

                switch (address & 0xff0f)
                {
                case 0x4300: dma.control = data; return;
                case 0x4301: dma.address_b = data; return;
                case 0x4302: dma.address_a = (dma.address_a & 0xffff00) | (data << 0); return;
                case 0x4303: dma.address_a = (dma.address_a & 0xff00ff) | (data << 8); return;
                case 0x4304: dma.address_a = (dma.address_a & 0x00ffff) | (data << 16); return;
                case 0x4305: dma.count = (ushort)((dma.count & 0xff00) | (data << 0)); return;
                case 0x4306: dma.count = (ushort)((dma.count & 0x00ff) | (data << 8)); return;
                case 0x4307: return;
                case 0x4308: return;
                case 0x4309: return;
                case 0x430a: return;
                case 0x430b: return;
                case 0x430c: return;
                case 0x430d: return;
                case 0x430e: return;
                case 0x430f: return;
                }
            }

            switch (address)
            {
            case 0x4016: return;

            case 0x4200:
                state.scpu.reg4200 = data;
                Cpu.NmiWrapper((state.scpu.reg4200 & 0x80) != 0);
                return;

            case 0x4201: return; // I/O Port
            case 0x4202: state.scpu.wrmpya = data; return; // WRMPYA
            case 0x4203: state.scpu.wrmpyb = data; state.scpu.rdmpy = (ushort)(state.scpu.wrmpya * state.scpu.wrmpyb); return; // WRMPYB
            case 0x4204: state.scpu.wrdiv = (ushort)((state.scpu.wrdiv & 0xff00) | (data << 0)); return; // WRDIVL
            case 0x4205: state.scpu.wrdiv = (ushort)((state.scpu.wrdiv & 0x00ff) | (data << 8)); return; // WRDIVH
            case 0x4206:
                state.scpu.wrdivb = data;

                if (state.scpu.wrdivb == 0)
                {
                    state.scpu.rddiv = 0xffff;
                    state.scpu.rdmpy = state.scpu.wrdiv;
                }
                else
                {
                    state.scpu.rddiv = (ushort)(state.scpu.wrdiv / state.scpu.wrdivb);
                    state.scpu.rdmpy = (ushort)(state.scpu.wrdiv % state.scpu.wrdivb);
                }
                return; // WRDIVB

            case 0x4207: state.scpu.h_target = (state.scpu.h_target & ~0x00ff) | (data << 0); return;
            case 0x4208: state.scpu.h_target = (state.scpu.h_target & ~0xff00) | (data << 8); return;
            case 0x4209: state.scpu.v_target = (state.scpu.v_target & ~0x00ff) | (data << 0); return;
            case 0x420a: state.scpu.v_target = (state.scpu.v_target & ~0xff00) | (data << 8); return;

            case 0x420b: /* MDMAEN */
                state.scpu.mdma_en = data;
                state.scpu.mdma_count = 2;
                return;

            case 0x420c: /* HDMAEN */
                state.scpu.hdma_en = data;
                return;

            case 0x420d:
                state.scpu.fast_cart = (data & 1) != 0;
                return;
            }

            throw new NotImplementedException($"Unknown address: ${bank:x2}:{address:x4}.");
        }
    }
}
