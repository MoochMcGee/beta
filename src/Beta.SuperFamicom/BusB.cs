using Beta.SuperFamicom.Memory;
using Beta.SuperFamicom.PPU;
using Beta.SuperFamicom.SMP;

namespace Beta.SuperFamicom
{
    public sealed class BusB
    {
        private readonly Ppu ppu;
        private readonly Smp smp;
        private readonly WRAM wram;

        public BusB(Ppu ppu, Smp smp, WRAM wram)
        {
            this.ppu = ppu;
            this.smp = smp;
            this.wram = wram;
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            if ((address & 0xc0) == 0x40)
            {
                // S-SMP Registers
                data = smp.ReadPort((ushort)(address & 3));
                return;
            }

            switch (address & 0xff)
            {
            // S-PPU Registers
            case 0x34: data = ppu.Peek2134(); break;
            case 0x35: data = ppu.Peek2135(); break;
            case 0x36: data = ppu.Peek2136(); break;
            case 0x37: data = ppu.Peek2137(); break;
            case 0x38: data = ppu.Peek2138(); break;
            case 0x39: data = ppu.Peek2139(); break;
            case 0x3a: data = ppu.Peek213A(); break;
            case 0x3b: data = ppu.Peek213B(); break;
            case 0x3c: data = ppu.Peek213C(); break;
            case 0x3d: data = ppu.Peek213D(); break;
            case 0x3e: data = ppu.Peek213E(); break;
            case 0x3f: data = ppu.Peek213F(); break;

            // W-RAM Registers
            case 0x80: data = wram.Read(); break;
            case 0x81: break;
            case 0x82: break;
            case 0x83: break;
            }
        }

        public void Write(byte bank, ushort address, byte data)
        {
            if ((address & 0xc0) == 0x40)
            {
                smp.WritePort((ushort)(address & 3), data);
                return;
            }

            switch (address & 0xff)
            {
            // S-PPU Registers
            case 0x00: ppu.Poke2100(data); break;
            case 0x01: ppu.Poke2101(data); break;
            case 0x02: ppu.Poke2102(data); break;
            case 0x03: ppu.Poke2103(data); break;
            case 0x04: ppu.Poke2104(data); break;
            case 0x05: ppu.Poke2105(data); break;
            case 0x06: ppu.Poke2106(data); break;
            case 0x07: ppu.Poke2107(data); break;
            case 0x08: ppu.Poke2108(data); break;
            case 0x09: ppu.Poke2109(data); break;
            case 0x0a: ppu.Poke210A(data); break;
            case 0x0b: ppu.Poke210B(data); break;
            case 0x0c: ppu.Poke210C(data); break;
            case 0x0d: ppu.Poke210D(data); break;
            case 0x0e: ppu.Poke210E(data); break;
            case 0x0f: ppu.Poke210F(data); break;
            case 0x10: ppu.Poke2110(data); break;
            case 0x11: ppu.Poke2111(data); break;
            case 0x12: ppu.Poke2112(data); break;
            case 0x13: ppu.Poke2113(data); break;
            case 0x14: ppu.Poke2114(data); break;
            case 0x15: ppu.Poke2115(data); break;
            case 0x16: ppu.Poke2116(data); break;
            case 0x17: ppu.Poke2117(data); break;
            case 0x18: ppu.Poke2118(data); break;
            case 0x19: ppu.Poke2119(data); break;
            case 0x1a: ppu.Poke211A(data); break;
            case 0x1b: ppu.Poke211B(data); break;
            case 0x1c: ppu.Poke211C(data); break;
            case 0x1d: ppu.Poke211D(data); break;
            case 0x1e: ppu.Poke211E(data); break;
            case 0x1f: ppu.Poke211F(data); break;
            case 0x20: ppu.Poke2120(data); break;
            case 0x21: ppu.Poke2121(data); break;
            case 0x22: ppu.Poke2122(data); break;
            case 0x23: ppu.Poke2123(data); break;
            case 0x24: ppu.Poke2124(data); break;
            case 0x25: ppu.Poke2125(data); break;
            case 0x26: ppu.Poke2126(data); break;
            case 0x27: ppu.Poke2127(data); break;
            case 0x28: ppu.Poke2128(data); break;
            case 0x29: ppu.Poke2129(data); break;
            case 0x2a: ppu.Poke212A(data); break;
            case 0x2b: ppu.Poke212B(data); break;
            case 0x2c: ppu.Poke212C(data); break;
            case 0x2d: ppu.Poke212D(data); break;
            case 0x2e: ppu.Poke212E(data); break;
            case 0x2f: ppu.Poke212F(data); break;
            case 0x30: ppu.Poke2130(data); break;
            case 0x31: ppu.Poke2131(data); break;
            case 0x32: ppu.Poke2132(data); break;
            case 0x33: ppu.Poke2133(data); break;

            // W-RAM Registers
            case 0x80: wram.Write(data); break;
            case 0x81: wram.address = (wram.address & 0x1ff00) | ((data << 0) & 0x000ff); break;
            case 0x82: wram.address = (wram.address & 0x100ff) | ((data << 8) & 0x0ff00); break;
            case 0x83: wram.address = (wram.address & 0x0ffff) | ((data << 16) & 0x10000); break;
            }
        }
    }
}
