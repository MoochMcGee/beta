namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private void Blend()
        {
            var layers = new Layer[]
            {
                bg0,
                bg1,
                bg2,
                bg3,
                spr
            };

            for (uint x = 0; x < 256; x++)
            {
                int color1 = cram.h[0];
                var color2 = fixedColor;
                var priority1 = 0;
                var priority2 = 0;
                var source1 = 5;
                var source2 = 5;

                for (int i = 0; i < 5; i++)
                {
                    var layer = layers[i];

                    if (layer.Enable[x])
                    {
                        if (layer.Sm != 0 && priority1 < layer.Priority[x]) { priority1 = layer.Priority[x]; source1 = i; }
                        if (layer.Ss != 0 && priority2 < layer.Priority[x]) { priority2 = layer.Priority[x]; source2 = i; }
                    }
                }

                var colorexempt = (source1 == 4 && (color1 < 0xc0));

                if (source1 != 5) color1 = cram.h[layers[source1].Raster[x]];
                if (source2 != 5) color2 = cram.h[layers[source2].Raster[x]];

                if (!colorexempt && mathEnable[source1])
                {
                    int r1 = (color1 >> 0) & 31, g1 = (color1 >> 5) & 31, b1 = (color1 >> 10) & 31;
                    int r2 = (color2 >> 0) & 31, g2 = (color2 >> 5) & 31, b2 = (color2 >> 10) & 31;

                    switch (mathType)
                    {
                    case 0:
                        r1 += r2;
                        g1 += g2;
                        b1 += b2;
                        break;

                    case 1:
                        r1 = (r1 + r2) / 2;
                        g1 = (g1 + g2) / 2;
                        b1 = (b1 + b2) / 2;
                        break;

                    case 2:
                        r1 -= r2;
                        g1 -= g2;
                        b1 -= b2;
                        break;

                    case 3:
                        r1 = (r1 - r2) / 2;
                        g1 = (g1 - g2) / 2;
                        b1 = (b1 - b2) / 2;
                        break;
                    }

                    if (r1 > 31) r1 = 31;
                    if (g1 > 31) g1 = 31;
                    if (b1 > 31) b1 = 31;
                    if (r1 < 0) r1 = 0;
                    if (g1 < 0) g1 = 0;
                    if (b1 < 0) b1 = 0;

                    color1 = (r1 << 0) | (g1 << 5) | (b1 << 10);
                }

                raster[x] = colors[color1];
            }
        }

        private void RenderMode0()
        {
            bg0.Render(Background.BPP2);
            bg1.Render(Background.BPP2);
            bg2.Render(Background.BPP2);
            bg3.Render(Background.BPP2);
            spr.Render();

            Blend();
        }

        private void RenderMode1()
        {
            bg0.Render(Background.BPP4);
            bg1.Render(Background.BPP4);
            bg2.Render(Background.BPP2);
            spr.Render();

            Blend();
        }

        private void RenderMode2()
        {
            /* Offset-per-tile */
        }

        private void RenderMode3()
        {
            bg0.Render(Background.BPP8);
            bg1.Render(Background.BPP4);
            spr.Render();

            for (int i = 0; i < 256; i++)
            {
                int color;

                if ((color = spr.GetColorM(i)) != 0) goto render; // Sprites with priority 3
                if ((color = bg0.GetColorM(i)) != 0) goto render; // BG1 tiles with priority 1
                if ((color = spr.GetColorM(i)) != 0) goto render; // Sprites with priority 2
                if ((color = bg1.GetColorM(i)) != 0) goto render; // BG2 tiles with priority 1
                if ((color = spr.GetColorM(i)) != 0) goto render; // Sprites with priority 1
                if ((color = bg0.GetColorM(i)) != 0) goto render; // BG1 tiles with priority 0
                if ((color = spr.GetColorM(i)) != 0) goto render; // Sprites with priority 0
                if ((color = bg1.GetColorM(i)) != 0) goto render; // BG2 tiles with priority 0

                render:
                raster[i] = colors[cram.h[color]];
            }
        }

        private void RenderMode4()
        {
            /* Offset-per-tile */
        }

        private void RenderMode5()
        {
            /* Hi-res */
        }

        private void RenderMode6()
        {
            /* Hi-res */
        }

        private void RenderMode7()
        {
            bg0.RenderAffine();
            spr.Render();

            Blend();
        }
    }
}
