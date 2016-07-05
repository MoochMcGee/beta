namespace Beta.GameBoyAdvance.PPU
{
    public partial class Ppu
    {
        private static int[] colorLut;

        private int[] window = new int[240];

        private void BlendLayers(int[] raster)
        {
            int backdrop = this.pram.h[0];

            for (var i = 0; i < 240; i++)
            {
                var color1 = backdrop;
                var color2 = backdrop;
                var mask1 = 0x20;
                var mask2 = 0x20;

                for (var p = 3; p >= 0; p--)
                {
                    if (bg3.MasterEnable && p == bg3.Priority && bg3.Enable[i]) { mask2 = mask1; color2 = color1; mask1 = 0x08; color1 = bg3.Raster[i] & 0x7fff; }
                    if (bg2.MasterEnable && p == bg2.Priority && bg2.Enable[i]) { mask2 = mask1; color2 = color1; mask1 = 0x04; color1 = bg2.Raster[i] & 0x7fff; }
                    if (bg1.MasterEnable && p == bg1.Priority && bg1.Enable[i]) { mask2 = mask1; color2 = color1; mask1 = 0x02; color1 = bg1.Raster[i] & 0x7fff; }
                    if (bg0.MasterEnable && p == bg0.Priority && bg0.Enable[i]) { mask2 = mask1; color2 = color1; mask1 = 0x01; color1 = bg0.Raster[i] & 0x7fff; }
                    if (spr.MasterEnable && p == spr.Priority[i] && spr.Enable[i]) { mask2 = mask1; color2 = color1; mask1 = 0x10; color1 = spr.Raster[i]; }
                }

                if ((window[i] & 0x20) != 0)
                {
                    var blend1 = (blend.Target1 & mask1) != 0;
                    var blend2 = (blend.Target2 & mask2) != 0;

                    if (color1 > 0x7fff && blend2) { color1 = BlendLayers(color1, blend.Eva, color2, blend.Evb); }
                    else if (blend.Type == 1 && blend1 && blend2) { color1 = BlendLayers(color1, blend.Eva, color2, blend.Evb); }
                    else if (blend.Type == 2 && blend1) { color1 = BlendLayers(color1, 16 - blend.Evy, 0x7fff, blend.Evy); }
                    else if (blend.Type == 3 && blend1) { color1 = BlendLayers(color1, 16 - blend.Evy, 0x0000, blend.Evy); }
                }

                raster[i] = colorLut[color1 & 0x7fff];
            }
        }

        private int BlendLayers(int color1, int eva, int color2, int evb)
        {
            int r1 = (color1 >> 0) & 31, g1 = (color1 >> 5) & 31, b1 = (color1 >> 10) & 31;
            int r2 = (color2 >> 0) & 31, g2 = (color2 >> 5) & 31, b2 = (color2 >> 10) & 31;

            var r = ((r1 * eva) + (r2 * evb)) >> 4;
            var g = ((g1 * eva) + (g2 * evb)) >> 4;
            var b = ((b1 * eva) + (b2 * evb)) >> 4;

            if (r > 31) r = 31;
            if (g > 31) g = 31;
            if (b > 31) b = 31;

            return (r << 0) | (g << 5) | (b << 10);
        }

        private void RenderMode0(int[] raster)
        {
            if (spr.MasterEnable) RenderSprite();
            if (bg0.MasterEnable) RenderLinear(bg0);
            if (bg1.MasterEnable) RenderLinear(bg1);
            if (bg2.MasterEnable) RenderLinear(bg2);
            if (bg3.MasterEnable) RenderLinear(bg3);

            BlendLayers(raster);
        }

        private void RenderMode1(int[] raster)
        {
            if (spr.MasterEnable) RenderSprite();
            if (bg0.MasterEnable) RenderLinear(bg0);
            if (bg1.MasterEnable) RenderLinear(bg1);
            if (bg2.MasterEnable) RenderAffine(bg2);

            BlendLayers(raster);
        }

        private void RenderMode2(int[] raster)
        {
            if (spr.MasterEnable) RenderSprite();
            if (bg2.MasterEnable) RenderAffine(bg2);
            if (bg3.MasterEnable) RenderAffine(bg3);

            BlendLayers(raster);
        }

        private void RenderMode3(int[] raster)
        {
            var vram = this.vram.h;

            if (spr.MasterEnable) RenderSprite();
            if (bg2.MasterEnable)
            {
                int dx = bg2.Dx;
                int dy = bg2.Dy;
                var rx = bg2.Rx;
                var ry = bg2.Ry;

                for (var i = 0; i < 240; i++, rx += dx, ry += dy)
                {
                    if ((window[i] & 0x04) == 0)
                    {
                        bg2.Enable[i] = false;
                        continue;
                    }

                    var ax = rx >> 8;
                    var ay = ry >> 8;

                    if (ax >= 0 && ax < 240 && ay >= 0 && ay < 160)
                    {
                        bg2.Enable[i] = true;
                        bg2.Raster[i] = vram[(ay * 240) + ax];
                    }
                    else
                    {
                        bg2.Enable[i] = false;
                    }
                }
            }

            BlendLayers(raster);
        }

        private void RenderMode4(int[] raster)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            if (spr.MasterEnable) RenderSprite();
            if (bg2.MasterEnable)
            {
                var addressBase = displayFrameSelect ? 0xa000 : 0x0000;
                int dx = bg2.Dx;
                int dy = bg2.Dy;
                var rx = bg2.Rx;
                var ry = bg2.Ry;

                for (var i = 0; i < 240; i++, rx += dx, ry += dy)
                {
                    if ((window[i] & 0x04) == 0)
                    {
                        bg2.Enable[i] = false;
                        continue;
                    }

                    var ax = rx >> 8;
                    var ay = ry >> 8;

                    if (ax >= 0 && ax < 240 && ay >= 0 && ay < 160)
                    {
                        int colour = vram[addressBase + (ay * 240) + ax];

                        if (colour != 0)
                        {
                            bg2.Enable[i] = true;
                            bg2.Raster[i] = pram[colour];
                        }
                        else
                        {
                            bg2.Enable[i] = false;
                        }
                    }
                }
            }

            BlendLayers(raster);
        }

        private void RenderMode5(int[] raster)
        {
            var vram = this.vram.h;

            if (spr.MasterEnable) RenderSprite();
            if (bg2.MasterEnable)
            {
                var addressBase = displayFrameSelect ? 0xa000 : 0x0000;
                int dx = bg2.Dx;
                int dy = bg2.Dy;
                var rx = bg2.Rx;
                var ry = bg2.Ry;

                for (var i = 0; i < 240; i++, rx += dx, ry += dy)
                {
                    if ((window[i] & 0x04) == 0)
                    {
                        bg2.Enable[i] = false;
                        continue;
                    }

                    var ax = rx >> 8;
                    var ay = ry >> 8;

                    if (ax >= 0 && ax < 160 && ay >= 0 && ay < 128)
                    {
                        bg2.Enable[i] = true;
                        bg2.Raster[i] = vram[addressBase + (ay * 160) + ax];
                    }
                    else
                    {
                        bg2.Enable[i] = false;
                    }
                }
            }

            BlendLayers(raster);
        }

        private void RenderAffine(Bg bg)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            var size = 0;
            var mask = 0;
            var exp = 0;

            switch (bg.Size)
            {
            case 0: mask = (size = 1 << 0x7) - 1; exp = 4; break;
            case 1: mask = (size = 1 << 0x8) - 1; exp = 5; break;
            case 2: mask = (size = 1 << 0x9) - 1; exp = 6; break;
            case 3: mask = (size = 1 << 0xA) - 1; exp = 7; break;
            }

            var nmtBase = (bg.NmtBase << 11); // $0000-$f800
            var chrBase = (bg.ChrBase << 14); // $0000-$c000
            int dx = bg.Dx;
            int dy = bg.Dy;
            var rx = bg.Rx;
            var ry = bg.Ry;

            var windowMask = (1 << bg.Index);

            for (var i = 0; i < 240; i++, rx += dx, ry += dy)
            {
                if ((window[i] & windowMask) == 0)
                {
                    bg.Enable[i] = false;
                    continue;
                }

                var ax = rx >> 8;
                var ay = ry >> 8;

                if ((ax >= 0 && ax < size && ay >= 0 && ay < size) || bg.Wrap)
                {
                    var address = nmtBase + (((ay & mask) / 8) << exp) + ((ax & mask) / 8);
                    int colour = vram[chrBase + (vram[address] << 0x6) + ((ay & 7) * 8) + (ax & 7)];

                    if (colour != 0)
                    {
                        bg.Enable[i] = true;
                        bg.Raster[i] = pram[colour];
                    }
                    else
                    {
                        bg.Enable[i] = false;
                    }
                }
            }
        }

        private void RenderLinear(Bg bg)
        {
            var pram16 = this.pram.h;
            var vram16 = this.vram.h;
            var vram8 = this.vram.b;

            var xMask = (bg.Size & 0x01) << 8;
            var yMask = (bg.Size & 0x02) << 7;

            var chrBase = (bg.ChrBase << 14);
            var nmtBase = (bg.NmtBase << 10);
            int xScroll = (bg.Scx);
            var yScroll = (bg.Scy + vclock);

            var baseAddr = nmtBase + ((yScroll & 0xf8) << 2);

            int xToggle = 0;
            int yToggle = 0;

            switch (bg.Size)
            {
            case 0: xToggle = 0x000; yToggle = 0x000; break; // 32x32
            case 1: xToggle = 0x400; yToggle = 0x000; break; // 64x32
            case 2: xToggle = 0x000; yToggle = 0x400; break; // 32x64
            case 3: xToggle = 0x400; yToggle = 0x800; break; // 64x64
            }

            if ((yScroll & yMask) != 0)
                baseAddr += yToggle;

            var windowMask = (1 << bg.Index);

            if (bg.Depth)
            {
                // 8bpp
                for (var i = 0; i < 240; i++, xScroll++)
                {
                    if ((window[i] & windowMask) == 0)
                    {
                        bg.Enable[i] = false;
                        continue;
                    }

                    var tileAddr = baseAddr + ((xScroll & 0xf8) >> 3);

                    if ((xScroll & xMask) != 0)
                    {
                        tileAddr += xToggle;
                    }

                    var tile = vram16[tileAddr];
                    var x = xScroll & 7;
                    var y = yScroll & 7;

                    if ((tile & 0x400) != 0) x ^= 7;
                    if ((tile & 0x800) != 0) y ^= 7;

                    int colour = vram8[chrBase + ((tile & 0x3ff) << 6) + (y << 3) + (x >> 0)];

                    if (colour != 0)
                    {
                        bg.Enable[i] = true;
                        bg.Raster[i] = pram16[colour];
                    }
                    else
                    {
                        bg.Enable[i] = false;
                    }
                }
            }
            else
            {
                // 4bpp
                for (var i = 0; i < 240; i++, xScroll++)
                {
                    if ((window[i] & windowMask) == 0)
                    {
                        bg.Enable[i] = false;
                        continue;
                    }

                    var tileAddr = baseAddr + ((xScroll & 0xf8) >> 3);

                    if ((xScroll & xMask) != 0)
                        tileAddr += xToggle;

                    int tile = vram16[tileAddr];
                    var x = xScroll & 7;
                    var y = yScroll & 7;

                    if ((tile & 0x400) != 0) x ^= 7;
                    if ((tile & 0x800) != 0) y ^= 7;

                    int colour = vram8[chrBase + ((tile & 0x3ff) << 5) + (y << 2) + (x >> 1)];

                    if ((x & 1) == 0)
                        colour &= 15;
                    else
                        colour >>= 4;

                    if (colour != 0)
                    {
                        bg.Enable[i] = true;
                        bg.Raster[i] = pram16[((tile >> 8) & 0xf0) + colour];
                    }
                    else
                    {
                        bg.Enable[i] = false;
                    }
                }
            }
        }

        private void RenderSprite()
        {
            var oram = this.oram.h;

            for (var sprite = 512 - 4; sprite >= 0; sprite -= 4)
            {
                var attr0 = oram[sprite | 0];
                var attr1 = oram[sprite | 1];
                var attr2 = oram[sprite | 2];

                var priority = (attr2 >> 10) & 3;
                var semitransparent = 0;

                var x = attr1 & 0x1ff;
                var y = attr0 & 0xff;

                switch ((attr0 >> 10) & 3)
                {
                case 0: break;
                case 1: semitransparent = 0x8000; break; // Semi-transparent
                case 2: continue; // Obj window
                case 3: continue;
                }

                int w = Sp.XSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3],
                    h = Sp.YSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3];

                int rw = w,
                    rh = h;

                switch (attr0 & 0x300)
                {
                case 0x000: break;    // Rot-scale off, sprite displayed
                case 0x100: break;    // Rot-scale on, normal size
                case 0x200: continue; // Rot-scale off, sprite hidden
                case 0x300: // Rot-scale on, double size
                    rw *= 2;
                    rh *= 2;
                    break;
                }

                var line = (vclock - y) & 0xff;
                if (line >= rh)
                    continue;

                if ((attr0 & 0x0100) == 0)
                {
                    if ((attr1 & 0x2000) != 0) line ^= (h - 1);
                    if ((attr0 & 0x2000) != 0)
                    {
                        RenderSprite8BppLinear(attr1, attr2, priority, semitransparent, x, w, line);
                    }
                    else
                    {
                        RenderSprite4BppLinear(attr1, attr2, priority, semitransparent, x, w, line);
                    }
                }
                else
                {
                    var scale = (attr0 & 0x2000) != 0 ? 2 : 1;
                    var param = (attr1 & 0x3e00) >> 5;

                    var dx = (short)oram[param | 0x3];
                    var dmx = (short)oram[param | 0x7];
                    var dy = (short)oram[param | 0xb];
                    var dmy = (short)oram[param | 0xf];

                    var baseSprite = attr2 & 0x3ff;
                    int pitch;

                    if (spr.Mapping)
                    { // 1 dimensional
                        pitch = (w / 8) * scale;
                    }
                    else
                    { // 2 dimensional
                        pitch = 0x20;
                    }

                    var cx = (rw / 2);
                    var cy = line - (rh / 2);
                    var rx = (cy * dmx) - (cx * dx) + (w << 7);
                    var ry = (cy * dmy) - (cx * dy) + (h << 7);

                    // Draw a rot/scale sprite
                    if ((attr0 & (1 << 13)) != 0)
                    {
                        RenderSprite8BppAffine(attr2, priority, semitransparent, x, w, h, rw, scale, dx, dy, baseSprite, pitch, rx, ry);
                    }
                    else
                    {
                        RenderSprite4BppAffine(attr2, priority, semitransparent, x, w, h, rw, scale, dx, dy, baseSprite, pitch, rx, ry);
                    }
                }
            }
        }

        private void RenderSpriteWindow()
        {
            var oram = this.oram.h;
            var vram = this.vram.b;

            for (var sprite = 512u - 4u; sprite < 512u; sprite -= 4u)
            {
                var attr0 = oram[sprite | 0u];
                var attr1 = oram[sprite | 1u];
                var attr2 = oram[sprite | 2u];

                var x = attr1 & 0x1ff;
                var y = attr0 & 0xff;

                switch ((attr0 >> 10) & 3)
                {
                case 0: continue;
                case 1: continue; // Semi-transparent
                case 2: break; // Obj window
                case 3: continue;
                }

                int w = Sp.XSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3],
                    h = Sp.YSizeLut[(attr0 >> 14) & 3][(attr1 >> 14) & 3];

                int rw = w,
                    rh = h;

                switch (attr0 & 0x300)
                {
                case 0x000: break; // Rot-scale off, sprite displayed
                case 0x100: break; // Rot-scale on, normal size
                case 0x200: // Rot-scale off, sprite hidden
                    continue;

                case 0x300: // Rot-scale on, double size
                    rw *= 2;
                    rh *= 2;
                    break;
                }

                var line = (vclock - y) & 0xff;

                if (line >= rh)
                    continue;

                if ((attr0 & 0x100) == 0)
                {
                    if ((attr1 & 0x2000) != 0) line = (h - 1) - line;

                    if ((attr0 & 0x2000) != 0)
                    {
                        int baseSprite;

                        if (spr.Mapping)
                        {
                            // 1 dimensional
                            baseSprite = (attr2 & 0x3ff) + ((line / 8) * (w / 8)) * 2;
                        }
                        else
                        {
                            // 2 dimensional
                            baseSprite = (attr2 & 0x3ff) + ((line / 8) * 0x20);
                        }

                        var baseInc = 2;

                        if ((attr1 & 0x1000) != 0)
                        {
                            baseSprite += ((w / 8) - 1) * 2;
                            baseInc = -2;
                        }

                        // 256 colors
                        for (var i = x; i < x + w; i++)
                        {
                            if ((i & 0x1FF) < 240)
                            {
                                var tx = (i - x) & 7;

                                if ((attr1 & 0x1000) != 0)
                                    tx ^= 7;

                                var address = (baseSprite << 5) + ((line & 7) << 3) + (tx >> 0);
                                int colour = vram[0x10000 + address];

                                if (colour != 0)
                                {
                                    window[(i & 0x1ff)] = windowObjFlags;
                                }
                            }

                            if (((i - x) & 7) == 7) baseSprite += baseInc;
                        }
                    }
                    else
                    {
                        int baseSprite;

                        if (spr.Mapping)
                        {
                            // 1 dimensional
                            baseSprite = (attr2 & 0x3ff) + ((line / 8) * (w / 8)) * 1;
                        }
                        else
                        {
                            // 2 dimensional
                            baseSprite = (attr2 & 0x3ff) + ((line / 8) * 0x20);
                        }

                        var baseInc = 1;

                        if ((attr1 & 0x1000) != 0)
                        {
                            baseSprite += ((w / 8) - 1) * 1;
                            baseInc = -baseInc;
                        }

                        for (var i = x; i < x + w; i++)
                        {
                            if ((i & 0x1ff) < 240)
                            {
                                var tx = (i - x) & 7;

                                if ((attr1 & (1 << 12)) != 0)
                                    tx ^= 7;

                                var address = (baseSprite << 5) + ((line & 7) << 2) + (tx >> 1);
                                int colour = vram[0x10000 + address];

                                if ((tx & 1) == 0)
                                {
                                    colour &= 15;
                                }
                                else
                                {
                                    colour >>= 4;
                                }

                                if (colour != 0)
                                {
                                    window[(i & 0x1ff)] = windowObjFlags;
                                }
                            }

                            if (((i - x) & 7) == 7) baseSprite += baseInc;
                        }
                    }
                }
                else
                {
                    var scale = (attr0 & 0x2000) != 0 ? 2 : 1;
                    var param = (attr1 & 0x3e00) >> 5;

                    var dx = (short)oram[param | 0x3];
                    var dmx = (short)oram[param | 0x7];
                    var dy = (short)oram[param | 0xb];
                    var dmy = (short)oram[param | 0xf];

                    var cx = rw / 2;
                    var cy = rh / 2;

                    var baseSprite = attr2 & 0x3ff;
                    int pitch;

                    if (spr.Mapping)
                    {
                        // 1 dimensional
                        pitch = (w / 8) * scale;
                    }
                    else
                    {
                        // 2 dimensional
                        pitch = 0x20;
                    }

                    var rx = ((line - cy) * dmx) - (cx * dx) + (w << 7);
                    var ry = ((line - cy) * dmy) - (cx * dy) + (h << 7);

                    // Draw a rot/scale sprite
                    if ((attr0 & (1 << 13)) != 0)
                    {
                        // 256 colors
                        for (var i = x; i < x + rw; i++)
                        {
                            var tx = rx >> 8;
                            var ty = ry >> 8;

                            rx += dx;
                            ry += dy;

                            if ((i & 0x1ff) < 240 && tx >= 0 && tx < w && ty >= 0 && ty < h)
                            {
                                var address = (baseSprite + ((ty / 8) * pitch) + ((tx / 8) * scale)) * 32 + ((ty & 7) * 8) + (tx & 7);
                                int colour = vram[0x10000 + address];

                                if (colour != 0)
                                {
                                    window[i & 0x1ff] = windowObjFlags;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 16 colors
                        for (var i = x; i < x + rw; i++)
                        {
                            var tx = rx >> 8;
                            var ty = ry >> 8;

                            rx += dx;
                            ry += dy;

                            if ((i & 0x1ff) >= 240)
                            {
                                continue;
                            }

                            if (tx >= 0 && tx < w &&
                                ty >= 0 && ty < h)
                            {
                                var address = (baseSprite + ((ty / 8) * pitch) + ((tx / 8) * scale)) * 32 + ((ty & 7) * 4) + ((tx & 7) / 2);
                                int colour = vram[0x10000 + address];

                                if ((tx & 1) == 0)
                                {
                                    colour &= 15;
                                }
                                else
                                {
                                    colour >>= 4;
                                }

                                if (colour != 0)
                                {
                                    window[i & 0x1ff] = windowObjFlags;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RenderSprite4BppAffine(ushort attr2, int priority, int semitransparent, int x, int w, int h, int rw, int scale, short dx, short dy, int baseSprite, int pitch, int rx, int ry)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            var palette = 0x100 + ((attr2 >> 8) & 0xf0);

            // 16 colors
            for (var i = x; i < x + rw; i++)
            {
                var tx = rx >> 8;
                var ty = ry >> 8;

                rx += dx;
                ry += dy;

                if ((i & 0x1ff) < 240 && tx >= 0 && tx < w && ty >= 0 && ty < h)
                {
                    if ((window[i & 0x1ff] & 0x10) == 0)
                        continue;

                    var address = (baseSprite + ((ty / 8) * pitch) + ((tx / 8) * scale)) * 32 + ((ty & 7) * 4) + ((tx & 7) / 2);
                    int colour = vram[0x10000 + address];

                    if ((tx & 1) == 0)
                    {
                        colour &= 15;
                    }
                    else
                    {
                        colour >>= 4;
                    }

                    if (colour != 0 && spr.Priority[i & 0x1ff] >= priority)
                    {
                        spr.Enable[i & 0x1ff] = true;
                        spr.Raster[i & 0x1ff] = (pram[palette + colour] & 0x7fff) | semitransparent;
                        spr.Priority[i & 0x1ff] = priority;
                    }
                }
            }
        }

        private void RenderSprite8BppAffine(ushort attr2, int priority, int semitransparent, int x, int w, int h, int rw, int scale, short dx, short dy, int baseSprite, int pitch, int rx, int ry)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            // 256 colors
            for (var i = x; i < x + rw; i++)
            {
                var tx = rx >> 8;
                var ty = ry >> 8;

                rx += dx;
                ry += dy;

                if ((i & 0x1ff) < 240 && tx >= 0 && tx < w && ty >= 0 && ty < h)
                {
                    if ((window[i & 0x1ff] & 0x10) == 0)
                        continue;

                    var address = (baseSprite + ((ty / 8) * pitch) + ((tx / 8) * scale)) * 32 + ((ty & 7) * 8) + (tx & 7);
                    int colour = vram[0x10000 + address];

                    if (colour != 0 && spr.Priority[i & 0x1ff] >= priority)
                    {
                        spr.Enable[i & 0x1ff] = true;
                        spr.Raster[i & 0x1ff] = (pram[0x100 + colour] & 0x7fff) | semitransparent;
                        spr.Priority[i & 0x1ff] = priority;
                    }
                }
            }
        }

        private void RenderSprite4BppLinear(ushort attr1, ushort attr2, int priority, int semitransparent, int x, int w, int line)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            int baseSprite;

            if (spr.Mapping)
            {
                // 1 dimensional
                baseSprite = (attr2 & 0x3FF) + ((line / 8) * (w / 8)) * 1;
            }
            else
            {
                // 2 dimensional
                baseSprite = (attr2 & 0x3FF) + ((line / 8) * 0x20);
            }

            var baseInc = 1;

            if ((attr1 & 0x1000) != 0)
            {
                baseSprite += ((w / 8) - 1) * 1;
                baseInc = -baseInc;
            }

            // 16 colors
            var palette = 0x100 + ((attr2 >> 8) & 0xF0);

            for (var i = x; i < x + w; i++)
            {
                if ((i & 0x1ff) < 240 && (window[(i & 0x1ff)] & 0x10) != 0)
                {
                    var tx = (i - x) & 7;

                    if ((attr1 & (1 << 12)) != 0)
                        tx ^= 7;

                    var address = (baseSprite << 5) + ((line & 7) << 2) + (tx >> 1);
                    int colour = vram[0x10000 + (address & 0x7fff)];

                    if ((tx & 1) == 0)
                    {
                        colour &= 15;
                    }
                    else
                    {
                        colour >>= 4;
                    }

                    if (colour != 0 && spr.Priority[i & 0x1ff] >= priority)
                    {
                        spr.Enable[i & 0x1ff] = true;
                        spr.Raster[i & 0x1ff] = (pram[palette + colour] & 0x7fff) | semitransparent;
                        spr.Priority[i & 0x1ff] = priority;
                    }
                }

                if (((i - x) & 7) == 7) baseSprite += baseInc;
            }
        }

        private void RenderSprite8BppLinear(ushort attr1, ushort attr2, int priority, int semitransparent, int x, int w, int line)
        {
            var pram = this.pram.h;
            var vram = this.vram.b;

            int baseSprite;

            if (spr.Mapping)
            {
                // 1 dimensional
                baseSprite = (attr2 & 0x3FF) + ((line / 8) * (w / 8)) * 2;
            }
            else
            {
                // 2 dimensional
                baseSprite = (attr2 & 0x3FF) + ((line / 8) * 0x20);
            }

            var baseInc = 2;

            if ((attr1 & 0x1000) != 0)
            {
                baseSprite += ((w / 8) - 1) * 2;
                baseInc = -2;
            }

            // 256 colors
            for (var i = x; i < x + w; i++)
            {
                if ((i & 0x1ff) < 240 && (window[(i & 0x1ff)] & 0x10) != 0)
                {
                    var tx = (i - x) & 7;

                    if ((attr1 & 0x1000) != 0)
                        tx ^= 7;

                    var address = (baseSprite << 5) + ((line & 7) << 3) + (tx >> 0);
                    int colour = vram[0x10000 + address];

                    if (colour != 0 && spr.Priority[i & 0x1ff] >= priority)
                    {
                        spr.Enable[i & 0x1ff] = true;
                        spr.Raster[i & 0x1ff] = (pram[0x100 + colour] & 0x7fff) | semitransparent;
                        spr.Priority[i & 0x1ff] = priority;
                    }
                }

                if (((i - x) & 7) == 7) baseSprite += baseInc;
            }
        }
    }
}
