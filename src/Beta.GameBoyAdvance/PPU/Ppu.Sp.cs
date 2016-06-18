namespace Beta.GameBoyAdvance.PPU
{
    public partial class Ppu
    {
        private class Sp : Layer
        {
            private const int COUNT = 128;

            public static int MosaicH;
            public static int MosaicV;
            public static int[][] XSizeLut;
            public static int[][] YSizeLut;

            private GameSystem gameSystem;
            private int[] raster0 = new int[240];
            private int[] raster1 = new int[240];
            private int[] raster2 = new int[240];
            private int[] raster3 = new int[240];

            public bool Mapping;

            static Sp()
            {
                XSizeLut = new[]
                {
                    new[] {  8, 16, 32, 64 }, // square
                    new[] { 16, 32, 32, 64 }, // wide
                    new[] {  8,  8, 16, 32 }, // tall
                    new[] {  8,  8,  8,  8 }
                };

                YSizeLut = new[]
                {
                    new[] {  8, 16, 32, 64 }, // square
                    new[] {  8,  8, 16, 32 }, // wide
                    new[] { 16, 32, 32, 64 }, // tall
                    new[] {  8,  8,  8,  8 }
                };
            }

            public Sp(GameSystem gameSystem)
            {
                this.gameSystem = gameSystem;
                Index = 4;
            }

            private void Render4BppLinear(int[] raster, ushort attr0, ushort attr1, ushort attr2)
            {
            }

            private void Render8BppLinear(int[] raster, ushort attr0, ushort attr1, ushort attr2)
            {
            }

            private void Render4BppAffine(int[] raster, ushort attr0, ushort attr1, ushort attr2, short pa, short pb, short pc, short pd)
            {
            }

            private void Render8BppAffine(int[] raster, ushort attr0, ushort attr1, ushort attr2, short pa, short pb, short pc, short pd)
            {
            }

            public void Render()
            {
                var oram16 = gameSystem.oram.h;

                for (var i = COUNT - 1; i >= 0; i--)
                {
                    var index = i << 2;
                    var attr0 = oram16[index | 0];
                    var attr1 = oram16[index | 1];
                    var attr2 = oram16[index | 2];

                    switch ((attr0 >> 10) & 3)
                    {
                    case 0: break;    // normal
                    case 1: continue; // semi-transparent
                    case 2: continue; // window
                    case 3: continue; // prohibited
                    }

                    var x = attr1 & 0x1ff;
                    var y = attr0 & 0xff;

                    if ((attr0 & (1 << 8)) != 0)
                    {
                        var param = (attr1 & 0x3e00) >> 5;

                        var dx = (short)oram16[param | 0x3];
                        var dmx = (short)oram16[param | 0x7];
                        var dy = (short)oram16[param | 0xb];
                        var dmy = (short)oram16[param | 0xf];

                        if ((attr0 & (1 << 13)) != 0)
                        {
                            switch ((attr2 >> 10) & 3)
                            {
                            case 0: Render8BppAffine(raster0, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 1: Render8BppAffine(raster1, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 2: Render8BppAffine(raster2, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 3: Render8BppAffine(raster3, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            }
                        }
                        else
                        {
                            switch ((attr2 >> 10) & 3)
                            {
                            case 0: Render4BppAffine(raster0, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 1: Render4BppAffine(raster1, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 2: Render4BppAffine(raster2, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            case 3: Render4BppAffine(raster3, attr0, attr1, attr2, dx, dmx, dy, dmy); break;
                            }
                        }
                    }
                    else
                    {
                        if ((attr0 & (1 << 9)) != 0) continue;
                        if ((attr0 & (1 << 13)) != 0)
                        {
                            switch ((attr2 >> 10) & 3)
                            {
                            case 0: Render8BppLinear(raster0, attr0, attr1, attr2); break;
                            case 1: Render8BppLinear(raster1, attr0, attr1, attr2); break;
                            case 2: Render8BppLinear(raster2, attr0, attr1, attr2); break;
                            case 3: Render8BppLinear(raster3, attr0, attr1, attr2); break;
                            }
                        }
                        else
                        {
                            switch ((attr2 >> 10) & 3)
                            {
                            case 0: Render4BppLinear(raster0, attr0, attr1, attr2); break;
                            case 1: Render4BppLinear(raster1, attr0, attr1, attr2); break;
                            case 2: Render4BppLinear(raster2, attr0, attr1, attr2); break;
                            case 3: Render4BppLinear(raster3, attr0, attr1, attr2); break;
                            }
                        }
                    }
                }
            }

            // private void RenderSprite() {
            //     var oram = this.gameSystem.oram.h;
            //     var pram = this.gameSystem.pram.h;
            //     var vram = this.gameSystem.vram.b;
            //
            //     for (int sprite = 512 - 4; sprite >= 0; sprite -= 4) {
            //         ushort attr0 = oram[sprite | 0];
            //         ushort attr1 = oram[sprite | 1];
            //         ushort attr2 = oram[sprite | 2];
            //
            //         int priority = (attr2 >> 10) & 3;
            //         int semitransparent = 0;
            //
            //         int x = attr1 & 0x1ff;
            //         int y = attr0 & 0xff;
            //
            //         switch ((attr0 >> 10) & 3) {
            //         case 0: break;
            //         case 1: semitransparent = 0x8000; break; // Semi-transparent
            //         case 2: continue; // Obj window
            //         case 3: continue;
            //         }
            //
            //         int w = Sp.XSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3],
            //             h = Sp.YSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3];
            //
            //         int rw = w,
            //             rh = h;
            //
            //         switch (attr0 & 0x300) {
            //         case 0x000: break;    // Rot-scale off, sprite displayed
            //         case 0x100: break;    // Rot-scale on, normal size
            //         case 0x200: continue; // Rot-scale off, sprite hidden
            //         case 0x300: // Rot-scale on, double size
            //             rw *= 2;
            //             rh *= 2;
            //             break;
            //         }
            //
            //         int line = (this.vclock - y) & 0xff;
            //         if (line >= rh)
            //             continue;
            //
            //         if ((attr0 & 0x0100) == 0) {
            //             if ((attr1 & 0x2000) != 0) line ^= (h - 1);
            //             if ((attr0 & 0x2000) != 0) {
            //                 RenderSprite8bppLinear(attr1, attr2, priority, semitransparent, x, w, line);
            //             }
            //             else {
            //                 RenderSprite4bppLinear(attr1, attr2, priority, semitransparent, x, w, line);
            //             }
            //         }
            //         else {
            //             int scale = (attr0 & 0x2000) != 0 ? 2 : 1;
            //             int param = (attr1 & 0x3e00) >> 5;
            //
            //             short dx = (short)oram[param | 0x3];
            //             short dmx = (short)oram[param | 0x7];
            //             short dy = (short)oram[param | 0xb];
            //             short dmy = (short)oram[param | 0xf];
            //
            //             int baseSprite = attr2 & 0x3ff;
            //             int pitch;
            //
            //             if (this.spr.mapping) { // 1 dimensional
            //                 pitch = (w / 8) * scale;
            //             }
            //             else { // 2 dimensional
            //                 pitch = 0x20;
            //             }
            //
            //             int cx = (rw / 2);
            //             int cy = line - (rh / 2);
            //             int rx = (cy * dmx) - (cx * dx) + (w << 7);
            //             int ry = (cy * dmy) - (cx * dy) + (h << 7);
            //
            //             // Draw a rot/scale sprite
            //             if ((attr0 & (1 << 13)) != 0) {
            //                 RenderSprite8bppAffine(attr2, priority, semitransparent, x, w, h, rw, scale, dx, dy, baseSprite, pitch, rx, ry);
            //             }
            //             else {
            //                 RenderSprite4bppAffine(attr2, priority, semitransparent, x, w, h, rw, scale, dx, dy, baseSprite, pitch, rx, ry);
            //             }
            //         }
            //     }
            // }
        }
    }
}
