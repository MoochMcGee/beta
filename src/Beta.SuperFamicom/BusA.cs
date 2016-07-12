#define LOROM
using System;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP65816;
using Beta.SuperFamicom.Memory;

namespace Beta.SuperFamicom
{
    public sealed class BusA : IBus
    {
        private const int SpeedSlow = 12;
        private const int SpeedNorm = 8;
        private const int SpeedFast = 6;

        private readonly Driver gameSystem;
        private readonly SCpuState scpu;
        private readonly WRAM wram;
        private readonly byte[] cart;

        private byte open;
        private int speedCart = SpeedNorm;

        public BusA(Driver gameSystem, State state, byte[] cart)
        {
            this.gameSystem = gameSystem;
            this.scpu = state.scpu;
            this.cart = cart;
            this.wram = new WRAM();
        }

        public void Initialize()
        {
            AddCycles(170);
        }

        public void InternalOperation()
        {
            AddCycles(SpeedFast);
        }

        public byte Read(byte bank, ushort address)
        {
            var speed = GetSpeed(bank, address);
            AddCycles(speed);

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
                return gameSystem.Smp.ReadPort(address & 3, 0);
            }

            switch (address)
            {
            // S-PPU Registers
            case 0x2134: return gameSystem.Ppu.Peek2134();
            case 0x2135: return gameSystem.Ppu.Peek2135();
            case 0x2136: return gameSystem.Ppu.Peek2136();
            case 0x2137: return gameSystem.Ppu.Peek2137();
            case 0x2138: return gameSystem.Ppu.Peek2138();
            case 0x2139: return gameSystem.Ppu.Peek2139();
            case 0x213a: return gameSystem.Ppu.Peek213A();
            case 0x213b: return gameSystem.Ppu.Peek213B();
            case 0x213c: return gameSystem.Ppu.Peek213C();
            case 0x213d: return gameSystem.Ppu.Peek213D();
            case 0x213e: return gameSystem.Ppu.Peek213E();
            case 0x213f: return gameSystem.Ppu.Peek213F();

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

            case 0x4218: return gameSystem.Joypad1.Latch.l;
            case 0x4219: return gameSystem.Joypad1.Latch.h;

            case 0x421a: return gameSystem.Joypad2.Latch.l;
            case 0x421b: return gameSystem.Joypad2.Latch.h;

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
            AddCycles(speed);

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
                gameSystem.Smp.WritePort(address & 3, data, 0);
                return;
            }

            switch (address)
            {
            // S-PPU Registers
            case 0x2100: gameSystem.Ppu.Poke2100(data); break;
            case 0x2101: gameSystem.Ppu.Poke2101(data); break;
            case 0x2102: gameSystem.Ppu.Poke2102(data); break;
            case 0x2103: gameSystem.Ppu.Poke2103(data); break;
            case 0x2104: gameSystem.Ppu.Poke2104(data); break;
            case 0x2105: gameSystem.Ppu.Poke2105(data); break;
            case 0x2106: gameSystem.Ppu.Poke2106(data); break;
            case 0x2107: gameSystem.Ppu.Poke2107(data); break;
            case 0x2108: gameSystem.Ppu.Poke2108(data); break;
            case 0x2109: gameSystem.Ppu.Poke2109(data); break;
            case 0x210a: gameSystem.Ppu.Poke210A(data); break;
            case 0x210b: gameSystem.Ppu.Poke210B(data); break;
            case 0x210c: gameSystem.Ppu.Poke210C(data); break;
            case 0x210d: gameSystem.Ppu.Poke210D(data); break;
            case 0x210e: gameSystem.Ppu.Poke210E(data); break;
            case 0x210f: gameSystem.Ppu.Poke210F(data); break;
            case 0x2110: gameSystem.Ppu.Poke2110(data); break;
            case 0x2111: gameSystem.Ppu.Poke2111(data); break;
            case 0x2112: gameSystem.Ppu.Poke2112(data); break;
            case 0x2113: gameSystem.Ppu.Poke2113(data); break;
            case 0x2114: gameSystem.Ppu.Poke2114(data); break;
            case 0x2115: gameSystem.Ppu.Poke2115(data); break;
            case 0x2116: gameSystem.Ppu.Poke2116(data); break;
            case 0x2117: gameSystem.Ppu.Poke2117(data); break;
            case 0x2118: gameSystem.Ppu.Poke2118(data); break;
            case 0x2119: gameSystem.Ppu.Poke2119(data); break;
            case 0x211a: gameSystem.Ppu.Poke211A(data); break;
            case 0x211b: gameSystem.Ppu.Poke211B(data); break;
            case 0x211c: gameSystem.Ppu.Poke211C(data); break;
            case 0x211d: gameSystem.Ppu.Poke211D(data); break;
            case 0x211e: gameSystem.Ppu.Poke211E(data); break;
            case 0x211f: gameSystem.Ppu.Poke211F(data); break;
            case 0x2120: gameSystem.Ppu.Poke2120(data); break;
            case 0x2121: gameSystem.Ppu.Poke2121(data); break;
            case 0x2122: gameSystem.Ppu.Poke2122(data); break;
            case 0x2123: gameSystem.Ppu.Poke2123(data); break;
            case 0x2124: gameSystem.Ppu.Poke2124(data); break;
            case 0x2125: gameSystem.Ppu.Poke2125(data); break;
            case 0x2126: gameSystem.Ppu.Poke2126(data); break;
            case 0x2127: gameSystem.Ppu.Poke2127(data); break;
            case 0x2128: gameSystem.Ppu.Poke2128(data); break;
            case 0x2129: gameSystem.Ppu.Poke2129(data); break;
            case 0x212a: gameSystem.Ppu.Poke212A(data); break;
            case 0x212b: gameSystem.Ppu.Poke212B(data); break;
            case 0x212c: gameSystem.Ppu.Poke212C(data); break;
            case 0x212d: gameSystem.Ppu.Poke212D(data); break;
            case 0x212e: gameSystem.Ppu.Poke212E(data); break;
            case 0x212f: gameSystem.Ppu.Poke212F(data); break;
            case 0x2130: gameSystem.Ppu.Poke2130(data); break;
            case 0x2131: gameSystem.Ppu.Poke2131(data); break;
            case 0x2132: gameSystem.Ppu.Poke2132(data); break;
            case 0x2133: gameSystem.Ppu.Poke2133(data); break;

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
                gameSystem.Cpu.NmiWrapper((scpu.reg4200 & 0x80) != 0);
                return;

            case 0x4201: return; // I/O Port
            case 0x4202: scpu.wrmpya = data; return; // WRMPYA
            case 0x4203: scpu.wrmpyb = data; scpu.rdmpy = (ushort)(scpu.wrmpya * scpu.wrmpyb); return; // WRMPYB
            case 0x4204: scpu.wrdiv.l = data; return; // WRDIVL
            case 0x4205: scpu.wrdiv.h = data; return; // WRDIVH
            case 0x4206:
                scpu.wrdivb = data;

                if (scpu.wrdivb == 0)
                {
                    scpu.rddiv = 0xffff;
                    scpu.rdmpy = scpu.wrdiv.w;
                }
                else
                {
                    scpu.rddiv = (ushort)(scpu.wrdiv.w / scpu.wrdivb);
                    scpu.rdmpy = (ushort)(scpu.wrdiv.w % scpu.wrdivb);
                }
                return; // WRDIVB

            case 0x4207: scpu.h_target = (scpu.h_target & ~0x00ff) | (data << 0); return;
            case 0x4208: scpu.h_target = (scpu.h_target & ~0xff00) | (data << 8); return;
            case 0x4209: scpu.v_target = (scpu.v_target & ~0x00ff) | (data << 0); return;
            case 0x420a: scpu.v_target = (scpu.v_target & ~0xff00) | (data << 8); return;

            case 0x420b: /* MDMAEN */
                gameSystem.Dma.mdma_en = data;
                gameSystem.Dma.mdma_count = 2;
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

        public void AddCycles(int amount)
        {
            var clock = new ClockSignal(amount);

            gameSystem.Cpu.Consume(clock);
            gameSystem.Ppu.Consume(clock);
            gameSystem.Smp.Consume(clock);
        }
    }
}
