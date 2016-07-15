using System;
using Beta.Platform.Messaging;
using Beta.SuperFamicom.Cartridges;
using Beta.SuperFamicom.Memory;

namespace Beta.SuperFamicom
{
    public sealed class BusA
    {
        private readonly IProducer<ClockSignal> clock;
        private readonly State state;
        private readonly WRAM wram;

        private ICartridge cart;

        public Driver Driver;

        public BusA(IProducer<ClockSignal> clock, State state, WRAM wram)
        {
            this.clock = clock;
            this.state = state;
            this.wram = wram;
        }

        public void Initialize(ICartridge cart)
        {
            this.cart = cart;

            clock.Produce(new ClockSignal(170));
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            if ((bank & 0x7e) == 0x7e) { wram.Read(bank, address, ref data); return; }
            if ((bank & 0x7f) <= 0x3f)
            {
                if ((address & 0xe000) == 0x0000) { wram.Read(0x00, address, ref data); return; }
                if ((address & 0xff00) == 0x2100) { ReadBusB(bank, address, ref data); return; }
                if ((address & 0xfc00) == 0x4000) { ReadSCPU(bank, address, ref data); return; }
                if ((address & 0x8000) == 0x8000) { cart.Read(bank, address, ref data); return; }
                return;
            }

            if ((bank & 0x7f) <= 0x7f) { cart.Read(bank, address, ref data); return; }

            throw new NotImplementedException();
        }

        private void ReadBusB(byte bank, ushort address, ref byte data)
        {
            if ((address & 0xffc0) == 0x2140)
            {
                // S-SMP Registers
                data = Driver.Smp.ReadPort(address & 3, 0);
                return;
            }

            switch (address)
            {
            // S-PPU Registers
            case 0x2134: data = Driver.Ppu.Peek2134(); break;
            case 0x2135: data = Driver.Ppu.Peek2135(); break;
            case 0x2136: data = Driver.Ppu.Peek2136(); break;
            case 0x2137: data = Driver.Ppu.Peek2137(); break;
            case 0x2138: data = Driver.Ppu.Peek2138(); break;
            case 0x2139: data = Driver.Ppu.Peek2139(); break;
            case 0x213a: data = Driver.Ppu.Peek213A(); break;
            case 0x213b: data = Driver.Ppu.Peek213B(); break;
            case 0x213c: data = Driver.Ppu.Peek213C(); break;
            case 0x213d: data = Driver.Ppu.Peek213D(); break;
            case 0x213e: data = Driver.Ppu.Peek213E(); break;
            case 0x213f: data = Driver.Ppu.Peek213F(); break;

            // W-RAM Registers
            case 0x2180: data = wram.Read(); break;
            case 0x2181: break;
            case 0x2182: break;
            case 0x2183: break;
            }
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
                if ((address & 0xff00) == 0x2100) { WriteBusB(bank, address, data); return; } // $2100-$21ff
                if ((address & 0xfc00) == 0x4000) { WriteSCPU(bank, address, data); return; } // $4000-$43ff
                if ((address & 0x8000) == 0x8000) { cart.Write(bank, address, data); return; } // $8000-$ffff
                return;
            }

            if ((bank & 0x7f) <= 0x7f) { cart.Write(bank, address, data); return; }

            throw new NotImplementedException();
        }

        private void WriteBusB(byte bank, ushort address, byte data)
        {
            if ((address & 0xffc0) == 0x2140)
            {
                Driver.Smp.WritePort(address & 3, data, 0);
                return;
            }

            switch (address)
            {
            // S-PPU Registers
            case 0x2100: Driver.Ppu.Poke2100(data); break;
            case 0x2101: Driver.Ppu.Poke2101(data); break;
            case 0x2102: Driver.Ppu.Poke2102(data); break;
            case 0x2103: Driver.Ppu.Poke2103(data); break;
            case 0x2104: Driver.Ppu.Poke2104(data); break;
            case 0x2105: Driver.Ppu.Poke2105(data); break;
            case 0x2106: Driver.Ppu.Poke2106(data); break;
            case 0x2107: Driver.Ppu.Poke2107(data); break;
            case 0x2108: Driver.Ppu.Poke2108(data); break;
            case 0x2109: Driver.Ppu.Poke2109(data); break;
            case 0x210a: Driver.Ppu.Poke210A(data); break;
            case 0x210b: Driver.Ppu.Poke210B(data); break;
            case 0x210c: Driver.Ppu.Poke210C(data); break;
            case 0x210d: Driver.Ppu.Poke210D(data); break;
            case 0x210e: Driver.Ppu.Poke210E(data); break;
            case 0x210f: Driver.Ppu.Poke210F(data); break;
            case 0x2110: Driver.Ppu.Poke2110(data); break;
            case 0x2111: Driver.Ppu.Poke2111(data); break;
            case 0x2112: Driver.Ppu.Poke2112(data); break;
            case 0x2113: Driver.Ppu.Poke2113(data); break;
            case 0x2114: Driver.Ppu.Poke2114(data); break;
            case 0x2115: Driver.Ppu.Poke2115(data); break;
            case 0x2116: Driver.Ppu.Poke2116(data); break;
            case 0x2117: Driver.Ppu.Poke2117(data); break;
            case 0x2118: Driver.Ppu.Poke2118(data); break;
            case 0x2119: Driver.Ppu.Poke2119(data); break;
            case 0x211a: Driver.Ppu.Poke211A(data); break;
            case 0x211b: Driver.Ppu.Poke211B(data); break;
            case 0x211c: Driver.Ppu.Poke211C(data); break;
            case 0x211d: Driver.Ppu.Poke211D(data); break;
            case 0x211e: Driver.Ppu.Poke211E(data); break;
            case 0x211f: Driver.Ppu.Poke211F(data); break;
            case 0x2120: Driver.Ppu.Poke2120(data); break;
            case 0x2121: Driver.Ppu.Poke2121(data); break;
            case 0x2122: Driver.Ppu.Poke2122(data); break;
            case 0x2123: Driver.Ppu.Poke2123(data); break;
            case 0x2124: Driver.Ppu.Poke2124(data); break;
            case 0x2125: Driver.Ppu.Poke2125(data); break;
            case 0x2126: Driver.Ppu.Poke2126(data); break;
            case 0x2127: Driver.Ppu.Poke2127(data); break;
            case 0x2128: Driver.Ppu.Poke2128(data); break;
            case 0x2129: Driver.Ppu.Poke2129(data); break;
            case 0x212a: Driver.Ppu.Poke212A(data); break;
            case 0x212b: Driver.Ppu.Poke212B(data); break;
            case 0x212c: Driver.Ppu.Poke212C(data); break;
            case 0x212d: Driver.Ppu.Poke212D(data); break;
            case 0x212e: Driver.Ppu.Poke212E(data); break;
            case 0x212f: Driver.Ppu.Poke212F(data); break;
            case 0x2130: Driver.Ppu.Poke2130(data); break;
            case 0x2131: Driver.Ppu.Poke2131(data); break;
            case 0x2132: Driver.Ppu.Poke2132(data); break;
            case 0x2133: Driver.Ppu.Poke2133(data); break;

            // W-RAM Registers
            case 0x2180: wram.Write(data); break;
            case 0x2181: wram.address = (wram.address & 0x1ff00) | ((data << 0) & 0x000ff); break;
            case 0x2182: wram.address = (wram.address & 0x100ff) | ((data << 8) & 0x0ff00); break;
            case 0x2183: wram.address = (wram.address & 0x0ffff) | ((data << 16) & 0x10000); break;
            }
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
                Driver.Cpu.NmiWrapper((state.scpu.reg4200 & 0x80) != 0);
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
