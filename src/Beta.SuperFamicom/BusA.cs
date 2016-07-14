#define LOROM
using System;
using Beta.Platform.Messaging;
using Beta.SuperFamicom.Memory;

namespace Beta.SuperFamicom
{
    public sealed class BusA
    {
        private const int SpeedSlow = 12;
        private const int SpeedNorm = 8;
        private const int SpeedFast = 6;

        private readonly IProducer<ClockSignal> clock;
        private readonly SCpuState scpu;
        private readonly State state;
        private readonly WRAM wram;

        private byte open;
        private byte[] cart;
        private int speedCart = SpeedNorm;

        public Driver Driver;

        public BusA(IProducer<ClockSignal> clock, State state)
        {
            this.clock = clock;
            this.scpu = state.scpu;
            this.state = state;
            this.wram = new WRAM();
        }

        public void Initialize(byte[] cart)
        {
            this.cart = cart;

            clock.Produce(new ClockSignal(170));
        }

        public byte Read(byte bank, ushort address)
        {
            var speed = GetSpeed(bank, address);
            clock.Produce(new ClockSignal(speed));

            return open = ReadFree(bank, address);
        }

        public byte ReadFree(byte bank, ushort address)
        {
            if (bank == 0x7e) { return wram.Read(bank, address); }
            if (bank == 0x7f) { return wram.Read(bank, address); }

            if ((bank & 0x7f) <= 0x3f)
            {
                if ((address & 0xe000) == 0x0000) { return wram.Read(0x00, address); }
                if ((address & 0xff00) == 0x2100) { return ReadBusB(bank, address); }
                if ((address & 0xfc00) == 0x4000) { return ReadSCPU(bank, address); }
                if ((address & 0x8000) == 0x8000) { return ReadCart(bank, address); }
                return open;
            }

            if ((bank & 0x7f) <= 0x7f) { return ReadCart(bank, address); }

            throw new NotImplementedException();
        }

        private byte ReadBusB(byte bank, ushort address)
        {
            if ((address & 0xffc0) == 0x2140)
            {
                // S-SMP Registers
                return Driver.Smp.ReadPort(address & 3, 0);
            }

            switch (address)
            {
            // S-PPU Registers
            case 0x2134: return Driver.Ppu.Peek2134();
            case 0x2135: return Driver.Ppu.Peek2135();
            case 0x2136: return Driver.Ppu.Peek2136();
            case 0x2137: return Driver.Ppu.Peek2137();
            case 0x2138: return Driver.Ppu.Peek2138();
            case 0x2139: return Driver.Ppu.Peek2139();
            case 0x213a: return Driver.Ppu.Peek213A();
            case 0x213b: return Driver.Ppu.Peek213B();
            case 0x213c: return Driver.Ppu.Peek213C();
            case 0x213d: return Driver.Ppu.Peek213D();
            case 0x213e: return Driver.Ppu.Peek213E();
            case 0x213f: return Driver.Ppu.Peek213F();

            // W-RAM Registers
            case 0x2180: return wram.Read();
            case 0x2181: return open;
            case 0x2182: return open;
            case 0x2183: return open;
            }

            return open;
        }

        private byte ReadCart(byte bank, ushort address)
        {
#if LOROM
            var index = (bank << 15) | (address & 0x7fff);
#else
            var index = (bank << 16) | (address & 0xffff);
#endif
            return cart[index & (cart.Length - 1)];
        }

        private byte ReadSCPU(byte bank, ushort address)
        {
            switch (address)
            {
            case 0x4016: return 0; // JOYSER0
            case 0x4017: return 0; // JOYSER1

            case 0x4210: return (byte)((open & 0x70) | (scpu.in_vblank ? 0x80 : 0) | 0x02);
            case 0x4211: return (byte)((open & 0x7f) | (scpu.timer_coincidence ? 0x80 : 0));
            case 0x4212: return (byte)((open & 0x3e) | (scpu.in_vblank ? 0x80 : 0) | (scpu.in_hblank ? 0x40 : 0));
            case 0x4213: return 0; // I/O Port
            case 0x4214: return (byte)(scpu.rddiv >> 0); // RDDIVL
            case 0x4215: return (byte)(scpu.rddiv >> 8); // RDDIVH
            case 0x4216: return (byte)(scpu.rdmpy >> 0); // RDMPYL
            case 0x4217: return (byte)(scpu.rdmpy >> 8); // RDMPYH

            case 0x4218: return (byte)(state.pads[0] >> 0);
            case 0x4219: return (byte)(state.pads[0] >> 8);

            case 0x421a: return (byte)(state.pads[1] >> 0);
            case 0x421b: return (byte)(state.pads[1] >> 8);

            case 0x421c: return 0; // JOY3L
            case 0x421d: return 0; // JOY3H

            case 0x421e: return 0; // JOY4L
            case 0x421f: return 0; // JOY4H
            }

            return open;
            // throw new NotImplementedException($"Unknown address: ${bank:x2}:{address:x4}.");
        }

        public void Write(byte bank, ushort address, byte data)
        {
            var speed = GetSpeed(bank, address);
            clock.Produce(new ClockSignal(speed));

            WriteFree(bank, address, open = data);
        }

        public void WriteFree(byte bank, ushort address, byte data)
        {
            if (bank == 0x7e) { wram.Write(bank, address, data); return; }
            if (bank == 0x7f) { wram.Write(bank, address, data); return; }

            if ((bank & 0x7f) <= 0x3f)
            {
                if ((address & 0xe000) == 0x0000) { wram.Write(0x00, address, data); return; } // $0000-$1fff
                if ((address & 0xff00) == 0x2100) { WriteBusB(bank, address, data); return; } // $2100-$21ff
                if ((address & 0xfc00) == 0x4000) { WriteSCPU(bank, address, data); return; } // $4000-$43ff
                if ((address & 0x8000) == 0x8000) { WriteCart(bank, address, data); return; } // $8000-$ffff
                return;
            }

            if ((bank & 0x7f) <= 0x7f) { WriteCart(bank, address, data); return; }

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
            case 0x2181: wram.address = (wram.address & 0x1ff00) | ((data <<  0) & 0x000ff); break;
            case 0x2182: wram.address = (wram.address & 0x100ff) | ((data <<  8) & 0x0ff00); break;
            case 0x2183: wram.address = (wram.address & 0x0ffff) | ((data << 16) & 0x10000); break;
            }
        }

        private void WriteCart(byte bank, ushort address, byte data) { }

        private void WriteSCPU(byte bank, ushort address, byte data)
        {
            if ((address & 0xff00) == 0x4300)
            {
                var dma = scpu.dma[(address >> 4) & 7];

                switch (address & 0xff0f)
                {
                case 0x4300: dma.control = data; return;
                case 0x4301: dma.address_b = data; return;
                case 0x4302: dma.address_a.l = data; return;
                case 0x4303: dma.address_a.h = data; return;
                case 0x4304: dma.address_a.b = data; return;
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
                scpu.reg4200 = data;
                Driver.Cpu.NmiWrapper((scpu.reg4200 & 0x80) != 0);
                return;

            case 0x4201: return; // I/O Port
            case 0x4202: scpu.wrmpya = data; return; // WRMPYA
            case 0x4203: scpu.wrmpyb = data; scpu.rdmpy = (ushort)(scpu.wrmpya * scpu.wrmpyb); return; // WRMPYB
            case 0x4204: scpu.wrdiv = (ushort)((scpu.wrdiv & 0xff00) | (data << 0)); return; // WRDIVL
            case 0x4205: scpu.wrdiv = (ushort)((scpu.wrdiv & 0x00ff) | (data << 8)); return; // WRDIVH
            case 0x4206:
                scpu.wrdivb = data;

                if (scpu.wrdivb == 0)
                {
                    scpu.rddiv = 0xffff;
                    scpu.rdmpy = scpu.wrdiv;
                }
                else
                {
                    scpu.rddiv = (ushort)(scpu.wrdiv / scpu.wrdivb);
                    scpu.rdmpy = (ushort)(scpu.wrdiv % scpu.wrdivb);
                }
                return; // WRDIVB

            case 0x4207: scpu.h_target = (scpu.h_target & ~0x00ff) | (data << 0); return;
            case 0x4208: scpu.h_target = (scpu.h_target & ~0xff00) | (data << 8); return;
            case 0x4209: scpu.v_target = (scpu.v_target & ~0x00ff) | (data << 0); return;
            case 0x420a: scpu.v_target = (scpu.v_target & ~0xff00) | (data << 8); return;

            case 0x420b: /* MDMAEN */
                Driver.Dma.mdma_en = data;
                Driver.Dma.mdma_count = 2;
                return;

            case 0x420c: /* HDMAEN */
                return;

            case 0x420d:
                speedCart = (data & 1) == 1 ? SpeedFast : SpeedNorm;
                return;
            }

            throw new NotImplementedException($"Unknown address: ${bank:x2}:{address:x4}.");
        }

        private int GetSpeed(byte bank, ushort address)
        {
            var addr = (bank << 16) | address;

            if ((addr & 0x408000) != 0)
            {
                if ((addr & 0x800000) != 0) return speedCart;

                return SpeedNorm;
            }

            if (((addr + 0x6000) & 0x4000) != 0) return SpeedNorm;
            if (((addr - 0x4000) & 0x7E00) != 0) return SpeedFast;

            return SpeedSlow;
        }
    }
}
