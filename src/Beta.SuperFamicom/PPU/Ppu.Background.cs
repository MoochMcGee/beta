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

            private readonly Ppu ppu;
            private readonly BackgroundState bg;

            public Background(BackgroundState bg, Ppu ppu)
                : base(ppu, 2)
            {
                this.ppu = ppu;
                this.bg = bg;
            }

            public void Render(int depth)
            {
                if (bg.char_size == 16)
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
                var sppu = ppu.sppu;

                var xLine = (bg.h_offset) & 7;
                var xTile = (bg.h_offset) >> 3;
                var yLine = (bg.v_offset + ppu.vclock) & 7;
                var yTile = (bg.v_offset + ppu.vclock) >> 3;
                var xMove = 0;
                var yMove = 0;

                var fine = bg.h_offset & 7;
                var offset = 0;

                switch (bg.name_size)
                {
                case 0: yMove = 31; xMove = 31; break;
                case 1: yMove = 31; xMove =  5; break;
                case 2: yMove =  5; xMove = 31; break;
                case 3: yMove =  6; xMove =  5; break;
                }

                var bits = new byte[depth];
                var vram_0 = ppu.vram_0;
                var vram_1 = ppu.vram_1;

                for (uint i = 0; i < 33; i++, xTile++)
                {
                    var nameAddress = bg.name_base +
                        ((yTile & 31) << 5) +
                        ((xTile & 31) << 0) +
                        ((yTile & 32) << yMove) +
                        ((xTile & 32) << xMove);

                    var name =
                        (vram_0[nameAddress & 0x7fff] << 0) |
                        (vram_1[nameAddress & 0x7fff] << 8);

                    var charAddress = bg.char_base + ((name & 0x3ff) * depth * 4) + yLine;

                    var priority = priorities[(name >> 13) & 1];

                    if ((name & 0x8000) != 0)
                    {
                        charAddress ^= 0x0007; // vflip
                    }

                    if ((name & 0x4000) == 0) // hflip
                    {
                        for (int j = 0; j < depth; j += 2)
                        {
                            var addr = (charAddress & 0x7fff) | (j << 2);
                            bits[j | 0] = Utility.ReverseLookup[vram_0[addr]];
                            bits[j | 1] = Utility.ReverseLookup[vram_1[addr]];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < depth; j += 2)
                        {
                            var addr = (charAddress & 0x7fff) | (j << 2);
                            bits[j | 0] = vram_0[addr];
                            bits[j | 1] = vram_1[addr];
                        }
                    }

                    var palette = ((name & 0x1c00) >> 10) << depth;

                    switch (sppu.bg_mode)
                    {
                    case 0: palette += bg.index << 5; break;
                    case 3:
                    case 4: palette *= bg.index; break;
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
                            enable[offset] = true;
                            raster[offset] = palette + colour;
                            base.priority[offset] = priority;
                        }
                        else
                        {
                            enable[offset] = false;
                        }
                    }

                    fine = 0;
                }
            }

            public void RenderAffine()
            {
                var sppu = ppu.sppu;

                int a = (short)sppu.m7.a;
                int b = (short)sppu.m7.b;
                int c = (short)sppu.m7.c;
                int d = (short)sppu.m7.d;

                var cx = ((sppu.m7.x & 0x1fff) ^ 0x1000) - 0x1000;
                var cy = ((sppu.m7.y & 0x1fff) ^ 0x1000) - 0x1000;

                var hoffs = ((sppu.m7.h_offset & 0x1fff) ^ 0x1000) - 0x1000;
                var voffs = ((sppu.m7.v_offset & 0x1fff) ^ 0x1000) - 0x1000;

                var h = hoffs - cx;
                var v = voffs - cy;
                var y = ppu.vclock;

                h = (h & 0x2000) != 0 ? (h | ~0x3ff) : (h & 0x3ff);
                v = (v & 0x2000) != 0 ? (v | ~0x3ff) : (v & 0x3ff);

                if ((sppu.m7.control & 0x02) != 0) y ^= 255;

                var tx = ((a * h) & ~63) + ((b * v) & ~63) + ((b * y) & ~63) + (cx << 8);
                var ty = ((c * h) & ~63) + ((d * v) & ~63) + ((d * y) & ~63) + (cy << 8);
                var dx = a;
                var dy = c;

                if ((sppu.m7.control & 0x01) != 0)
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

                    var tile = ppu.vram_0[((ry & ~7) << 4) + (rx >> 3)]; // ..yy yyyy yxxx xxxx
                    var data = ppu.vram_1[(tile << 6) + ((ry & 7) << 3) + (rx & 7)]; // ..dd dddd ddyy yxxx

                    enable[x] = true;
                    raster[x] = data;
                    base.priority[x] = priorities[0];
                }
            }
        }
    }
}
