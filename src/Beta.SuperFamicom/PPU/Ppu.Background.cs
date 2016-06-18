using Beta.Platform;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private Background bg0;
        private Background bg1;
        private Background bg2;
        private Background bg3;

        private sealed class Background : Layer
        {
            public const int BPP2 = 2;
            public const int BPP4 = 4;
            public const int BPP8 = 8;

            private static int offsetLatch;

            public static new bool Priority;
            public static int Mode;
            public static int MosaicSize;

            public static byte M7Control;
            public static byte M7Latch;
            public static Register16 M7A;
            public static Register16 M7B;
            public static Register16 M7C;
            public static Register16 M7D;
            public static Register16 M7X;
            public static Register16 M7Y;
            public static Register16 M7HOffset;
            public static Register16 M7VOffset;

            private int hOffset;
            private int vOffset;

            public bool Mosaic;

            public int NameBase;
            public int NameSize;
            public int CharBase;
            public int CharSize;

            private int index;

            public Background(Ppu ppu, int index)
                : base(ppu, 2)
            {
                this.index = index;
            }

            public void WriteHOffset(byte data)
            {
                hOffset = (data << 8) | (offsetLatch & ~7) | ((hOffset >> 8) & 7);
                offsetLatch = data;
            }

            public void WriteVOffset(byte data)
            {
                vOffset = (data << 8) | offsetLatch;
                offsetLatch = data;
            }

            public void Render(int depth)
            {
                if (CharSize == 16)
                {
                    RenderLarge(depth);
                }
                else
                {
                    RenderSmall(depth);
                }
            }

            private void RenderLarge(int depth)
            {
            }

            private void RenderSmall(int depth)
            {
                var xLine = (hOffset) & 7;
                var xTile = (hOffset) >> 3;
                var yLine = (vOffset + Ppu.vclock) & 7;
                var yTile = (vOffset + Ppu.vclock) >> 3;
                var xMove = 0;
                var yMove = 0;

                var fine = hOffset & 7;
                var offset = 0;

                switch (NameSize)
                {
                case 0: yMove = 31; xMove = 31; break;
                case 1: yMove = 31; xMove =  5; break;
                case 2: yMove =  5; xMove = 31; break;
                case 3: yMove =  6; xMove =  5; break;
                }

                var bits = new byte[depth];

                for (uint i = 0; i < 33; i++, xTile++)
                {
                    var nameAddress = NameBase +
                        ((yTile & 31) << 5) +
                        ((xTile & 31) << 0) +
                        ((yTile & 32) << yMove) +
                        ((xTile & 32) << xMove);

                    var name = Ppu.vram[nameAddress & 0x7fff].w;
                    var charAddress = CharBase + ((name & 0x3ff) * depth * 4) + yLine;

                    var priority = Priorities[(name >> 13) & 1];

                    if ((name & 0x8000) != 0)
                    {
                        charAddress ^= 0x0007; // vflip
                    }

                    if ((name & 0x4000) == 0) // hflip
                    {
                        for (int j = 0; j < depth; j += 2)
                        {
                            var data = Ppu.vram[(charAddress & 0x7fff) | (j << 2)];
                            bits[j | 0] = Utility.ReverseLookup[data.l];
                            bits[j | 1] = Utility.ReverseLookup[data.h];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < depth; j += 2)
                        {
                            var data = Ppu.vram[(charAddress & 0x7fff) | (j << 2)];
                            bits[j | 0] = data.l;
                            bits[j | 1] = data.h;
                        }
                    }

                    var palette = ((name & 0x1c00) >> 10) << depth;

                    switch (Mode)
                    {
                    case 0: palette += index << 5; break;
                    case 3:
                    case 4: palette *= index; break;
                    }

                    for (var k = 0; k < depth; k++)
                    {
                        bits[k] >>= fine;
                    }

                    for (var j = fine; j < 8 && offset < 256; j++, offset++)
                    {
                        var colour = 0;

                        for (var k = 0; k < depth; k++)
                        {
                            colour |= (bits[k] & 1) << k;
                            bits[k] >>= 1;
                        }

                        if (colour != 0)
                        {
                            Enable[offset] = true;
                            Raster[offset] = palette + colour;
                            base.Priority[offset] = priority;
                        }
                        else
                        {
                            Enable[offset] = false;
                        }
                    }

                    fine = 0;
                }
            }

            public void RenderAffine()
            {
                int a = (short)M7A.w;
                int b = (short)M7B.w;
                int c = (short)M7C.w;
                int d = (short)M7D.w;

                var cx = ((M7X.w & 0x1fff) ^ 0x1000) - 0x1000;
                var cy = ((M7Y.w & 0x1fff) ^ 0x1000) - 0x1000;

                var hoffs = ((M7HOffset.w & 0x1fff) ^ 0x1000) - 0x1000;
                var voffs = ((M7VOffset.w & 0x1fff) ^ 0x1000) - 0x1000;

                var h = hoffs - cx;
                var v = voffs - cy;
                var y = Ppu.vclock;

                h = (h & 0x2000) != 0 ? (h | ~0x3ff) : (h & 0x3ff);
                v = (v & 0x2000) != 0 ? (v | ~0x3ff) : (v & 0x3ff);

                if ((M7Control & 0x02) != 0) y ^= 255;

                var tx = ((a * h) & ~63) + ((b * v) & ~63) + ((b * y) & ~63) + (cx << 8);
                var ty = ((c * h) & ~63) + ((d * v) & ~63) + ((d * y) & ~63) + (cy << 8);
                var dx = a;
                var dy = c;

                if ((M7Control & 0x01) != 0)
                {
                    tx += (dx * 255);
                    ty += (dy * 255);
                    dx = -dx;
                    dy = -dy;
                }

                for (var x = 0; x < 256; x++, tx += dx, ty += dy)
                {
                    var rx = (tx >> 8) & 0x3ff;
                    var ry = (ty >> 8) & 0x3ff;

                    var tile = Ppu.vram[((ry & ~7) << 4) + (rx >> 3)].l; // ..yy yyyy yxxx xxxx
                    var data = Ppu.vram[(tile << 6) + ((ry & 7) << 3) + (rx & 7)].h; // ..dd dddd ddyy yxxx

                    Enable[x] = true;
                    Raster[x] = data;
                    base.Priority[x] = Priorities[0];
                }
            }
        }
    }
}
