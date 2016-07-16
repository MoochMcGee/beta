using Beta.Platform;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private Sprite spr;

        private sealed class Sprite : Layer
        {
            public static int[][] WidthLut = new[]
            {
                new[] {  8, 16 }, // 000 =  8x8  and 16x16 sprites
                new[] {  8, 32 }, // 001 =  8x8  and 32x32 sprites
                new[] {  8, 64 }, // 010 =  8x8  and 64x64 sprites
                new[] { 16, 32 }, // 011 = 16x16 and 32x32 sprites
                new[] { 16, 64 }, // 100 = 16x16 and 64x64 sprites
                new[] { 32, 64 }, // 101 = 32x32 and 64x64 sprites
                new[] { 16, 32 }, // 110 = 16x32 and 32x64 sprites
                new[] { 16, 32 }  // 111 = 16x32 and 32x32 sprites
            };

            public static int[][] HeightLut = new[]
            {
                new[] {  8, 16 }, // 000 =  8x8  and 16x16 sprites
                new[] {  8, 32 }, // 001 =  8x8  and 32x32 sprites
                new[] {  8, 64 }, // 010 =  8x8  and 64x64 sprites
                new[] { 16, 32 }, // 011 = 16x16 and 32x32 sprites
                new[] { 16, 64 }, // 100 = 16x16 and 64x64 sprites
                new[] { 32, 64 }, // 101 = 32x32 and 64x64 sprites
                new[] { 32, 64 }, // 110 = 16x32 and 32x64 sprites
                new[] { 32, 32 }  // 111 = 16x32 and 32x32 sprites
            };

            private readonly Ppu ppu;

            public bool Interlace;
            public int Addr;
            public int Name;
            public int[] Width = WidthLut[0];
            public int[] Height = HeightLut[0];

            public Sprite(Ppu ppu)
                : base(ppu, 4)
            {
                this.ppu = ppu;
            }

            public void Render()
            {
                var t = 0;
                var r = 0;

                for (int i = 0; i < 256; i++)
                {
                    raster[i] = 0;
                    enable[i] = false;
                }

                for (var i = 127; i >= 0; i--)
                {
                    int exta = ppu.oam.h[(i >> 3) | 0x100] >> ((i & 7) << 1);
                    int xpos = ppu.oam.b[(i << 2) | 0x000];
                    int ypos = ppu.oam.b[(i << 2) | 0x001];
                    int tile = ppu.oam.b[(i << 2) | 0x002];
                    int attr = ppu.oam.b[(i << 2) | 0x003];

                    if (ypos >= 240)
                    {
                        ypos -= 256;
                    }

                    var line = (ppu.vclock - ypos - 1) & int.MaxValue;

                    xpos |= (exta & 1) << 8;

                    int xSize = Width[(exta & 2U) >> 1];
                    int ySize = Height[(exta & 2U) >> 1];

                    if (Interlace) line <<= 1;

                    if (line >= ySize)
                        continue;

                    r++;

                    var palette = 0x80 + ((attr & 0x0e) << 3);

                    if ((attr & 0x80) != 0)
                    {
                        line ^= (ySize - 1);
                    }

                    var charAddress = Addr + (tile << 4) + ((line & 0x38) << 5) + (line & 7);
                    var charStep = 16;

                    if ((attr & 0x40) != 0)
                    {
                        charAddress += (xSize - 8) << 1;
                        charStep = -16;
                    }

                    if ((attr & 0x01) != 0)
                    {
                        charAddress = (charAddress + Name) & 0x7fff;
                    }

                    var priority = priorities[(attr >> 4) & 3];

                    for (var column = 0; column < xSize; column += 8)
                    {
                        t++;

                        var bit0 = ppu.vram_0[charAddress + 0u];
                        var bit1 = ppu.vram_1[charAddress + 0u];
                        var bit2 = ppu.vram_0[charAddress + 8u];
                        var bit3 = ppu.vram_1[charAddress + 8u];

                        if ((attr & 0x40U) == 0U)
                        {
                            bit0 = Utility.ReverseLookup[bit0];
                            bit1 = Utility.ReverseLookup[bit1];
                            bit2 = Utility.ReverseLookup[bit2];
                            bit3 = Utility.ReverseLookup[bit3];
                        }

                        for (var x = 0; x < 8; x++, xpos = (xpos + 1) & 0x1ff)
                        {
                            var colour = ((bit0 & 1) << 0) | ((bit1 & 1) << 1) | ((bit2 & 1) << 2) | ((bit3 & 1) << 3);

                            bit0 >>= 1;
                            bit1 >>= 1;
                            bit2 >>= 1;
                            bit3 >>= 1;

                            if (colour == 0 || xpos >= 256)
                            {
                                continue;
                            }

                            enable[xpos] = true;
                            raster[xpos] = palette + colour;
                            base.priority[xpos] = priority;
                        }

                        charAddress = (charAddress + charStep) & 0x7fff;
                    }
                }

                if (t > 34) ppu.ppu1Stat |= 0x80;
                if (r > 32) ppu.ppu1Stat |= 0x40;
            }
        }
    }
}
