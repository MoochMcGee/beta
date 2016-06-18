using Beta.Platform;
using word = System.UInt16;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private static int[][] colorLookup = Utility.CreateArray<int>(16, 0x8000);

        private byte cramData; // latch for cgram
        private word cramAddr;
        private byte oramData; // latch for oram
        private word oramAddr;

        private Register16 vramAddr;
        private MemoryChip cram = new MemoryChip(0x0200); // 256x15-bit
        private MemoryChip oram = new MemoryChip(0x0220); // 256x16-bit + 16x16-bit
        private Register16[] vram = new Register16[0x8000]; // 32kw

        private byte vramCtrl;
        private byte vramStep = 0x01;

        private int MapVRamAddress(int address)
        {
            switch ((vramCtrl & 12) >> 2)
            {
            case 0: return (address & 0x7fff);
            case 1: return (address & 0x7f00) | ((address & 0x00e0) >> 5) | ((address & 0x001f) << 3);
            case 2: return (address & 0x7e00) | ((address & 0x01c0) >> 6) | ((address & 0x003f) << 3);
            case 3: return (address & 0x7c00) | ((address & 0x0380) >> 7) | ((address & 0x007f) << 3);
            }

            return 0;
        }

        public byte Peek213B()
        {
            var data = cram.b[cramAddr & 0x1ffu];

            cramAddr++;
            return data;
        }

        public byte Peek2138()
        {
            if ((oramAddr & 0x200) != 0)
            {
                // high table
                return oram.b[oramAddr++ & 0x21fu];
            }

            // low table
            return oram.b[oramAddr++ & 0x1ffu];
        }

        public byte Peek2139()
        {
            var data = vramLatch;
            vramLatch = vram[MapVRamAddress(vramAddr.w)].l;

            if ((vramCtrl & 0x80) == 0) { vramAddr.w += vramStep; }

            return ppu1Open = data;
        }

        public byte Peek213A()
        {
            var data = vramLatch;
            vramLatch = vram[MapVRamAddress(vramAddr.w)].h;

            if ((vramCtrl & 0x80) != 0) { vramAddr.w += vramStep; }

            return ppu1Open = data;
        }

        public void Poke2121(byte data)
        {
            cramAddr = (ushort)(data << 1);
        }

        public void Poke2122(byte data)
        {
            if ((cramAddr & 1) == 0)
            {
                cramData = data;
            }
            else
            {
                data &= 0x7f; // fix games that write $ffff for white

                cram.b[cramAddr & 0x1fe] = cramData;
                cram.b[cramAddr & 0x1ff] = data;
            }

            cramAddr++;
        }

        public void Poke2102(byte data)
        {
            oramAddr = (word)((oramAddr & ~0x1fe) | ((data & 0xff) << 1));
        }

        public void Poke2103(byte data)
        {
            oramAddr = (word)((oramAddr & ~0x200) | ((data & 0x01) << 9));
        }

        public void Poke2104(byte data)
        {
            if ((oramAddr & 0x200) != 0)
            {
                oram.b[oramAddr & 0x21f] = data;
            }
            else
            {
                if ((oramAddr & 1) == 0)
                {
                    oramData = data;
                }
                else
                {
                    oram.b[oramAddr & 0x1fe] = oramData;
                    oram.b[oramAddr & 0x1ff] = data;
                }
            }

            oramAddr++;
        }

        public void Poke2116(byte data)
        {
            vramAddr.l = data;
        }

        public void Poke2117(byte data)
        {
            vramAddr.h = data;
        }

        public void Poke2118(byte data)
        {
            var address = MapVRamAddress(vramAddr.w);

            vram[address].l = data;

            if ((vramCtrl & 0x80) == 0) { vramAddr.w += vramStep; }
        }

        public void Poke2119(byte data)
        {
            var address = MapVRamAddress(vramAddr.w);

            vram[address].h = data;

            if ((vramCtrl & 0x80) != 0) { vramAddr.w += vramStep; }
        }
    }
}
