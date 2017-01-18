using Beta.Platform;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private static int[][] colorLookup = Utility.CreateArray<int>(16, 0x8000);

        private byte cgram_data; // latch for cgram
        private int cgram_address;
        private byte oam_data; // latch for oam
        private int oam_address;
        private byte vram_data; // latch for vram
        private int vram_address;

        private MemoryChip cram = new MemoryChip(0x0200); // 256x15-bit
        private MemoryChip oam = new MemoryChip(0x0220); // 256x16-bit + 16x16-bit
        private byte[] vram_0 = new byte[0x8000];
        private byte[] vram_1 = new byte[0x8000];

        private byte vram_control;
        private byte vram_step = 0x01;

        private int MapVRamAddress(int address)
        {
            switch ((vram_control >> 2) & 3)
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
            var data = cram.b[cgram_address];

            cgram_address = (cgram_address + 1) & 0x1ff;

            return data;
        }

        public byte Peek2138()
        {
            var data = (oam_address > 0x1ff)
                ? oam.b[oam_address & 0x21f]
                : oam.b[oam_address & 0x1ff]
                ;

            oam_address = (oam_address + 1) & 0x3ff;

            return data;
        }

        public byte Peek2139()
        {
            var data = vram_data;
            vram_data = vram_0[MapVRamAddress(vram_address)];

            if ((vram_control & 0x80) == 0) { vram_address += vram_step; }

            return ppu1Open = data;
        }

        public byte Peek213A()
        {
            var data = vram_data;
            vram_data = vram_1[MapVRamAddress(vram_address)];

            if ((vram_control & 0x80) != 0) { vram_address += vram_step; }

            return ppu1Open = data;
        }

        public void Poke2121(byte data)
        {
            cgram_address = data << 1;
        }

        public void Poke2122(byte data)
        {
            if ((cgram_address & 1) == 0)
            {
                cgram_data = data;
            }
            else
            {
                data &= 0x7f; // fix games that write $ffff for white

                cram.b[cgram_address & 0x1fe] = cgram_data;
                cram.b[cgram_address & 0x1ff] = data;
            }

            cgram_address = (cgram_address + 1) & 0x1ff;
        }

        public void Poke2102(byte data)
        {
            oam_address = (oam_address & 0x200) | ((data << 1) & 0x1fe);
        }

        public void Poke2103(byte data)
        {
            oam_address = (oam_address & 0x1fe) | ((data << 9) & 0x200);
        }

        public void Poke2104(byte data)
        {
            if ((oam_address & 1) == 0)
            {
                oam_data = data;
            }

            if (oam_address > 0x1ff)
            {
                oam.b[oam_address & 0x21f] = data;
            }
            else
            {
                if ((oam_address & 1) != 0)
                {
                    oam.b[oam_address & 0x1fe] = oam_data;
                    oam.b[oam_address & 0x1ff] = data;
                }
            }

            oam_address = (oam_address + 1) & 0x3ff;
        }

        public void Poke2116(byte data)
        {
            vram_address = (vram_address & 0xff00) | (data << 0);
        }

        public void Poke2117(byte data)
        {
            vram_address = (vram_address & 0x00ff) | (data << 8);
        }

        public void Poke2118(byte data)
        {
            var address = MapVRamAddress(vram_address);

            vram_0[address] = data;

            if ((vram_control & 0x80) == 0) { vram_address += vram_step; }
        }

        public void Poke2119(byte data)
        {
            var address = MapVRamAddress(vram_address);

            vram_1[address] = data;

            if ((vram_control & 0x80) != 0) { vram_address += vram_step; }
        }
    }
}
