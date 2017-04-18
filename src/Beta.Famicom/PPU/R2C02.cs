using Beta.Famicom.Memory;
using Beta.Platform.Video;

namespace Beta.Famicom.PPU
{
    public static class R2C02
    {
        static int[] raster;

        static bool rendering(R2C02State e)
        {
            return (e.bkg_enabled || e.obj_enabled) && e.v < 240;
        }

        static void activeCycle(R2C02State e)
        {
            var h = e.h;

            if ((h & 0x100) == 0x000)
            {
                bgTick(e, h & 7);
                renderPixel(e);
                evaluationTick(e);
            }
            else if ((h & 0x1c0) == 0x100) { spTick(e, h & 7); }
            else if ((h & 0x1f0) == 0x140) { bgTick(e, h & 7); }
            else if ((h & 0x1fc) == 0x150) { bgTick(e, h & 1); }

            scrollTick(e);
        }

        static void evaluationTick(R2C02State e)
        {
            switch (e.h & 1)
            {
            case 0: SpUnit.evaluation0(e); break;
            case 1: SpUnit.evaluation1(e); break;
            }

            if (e.h == 0x3f) SpUnit.evaluationBegin(e);
            if (e.h == 0xff) SpUnit.evaluationReset(e);
        }

        static void bufferCycle(R2C02State e)
        {
            var h = e.h;

            if ((h & 0x100) == 0x000) { bgTick(e, h & 7); }
            else if ((h & 0x1c0) == 0x100) { spTick(e, h & 7); }
            else if ((h & 0x1f0) == 0x140) { bgTick(e, h & 7); }
            else if ((h & 0x1fc) == 0x150) { bgTick(e, h & 1); }

            scrollTick(e);

            if (h == 337 && e.field)
            {
                tickCounter(e);
            }
        }

        static void bgTick(R2C02State e, int step)
        {
            switch (step)
            {
            case 0: BgUnit.pointName(e); break;
            case 1: BgUnit.fetchName(e); break;
            case 2: BgUnit.pointAttr(e); break;
            case 3: BgUnit.fetchAttr(e); break;
            case 4: BgUnit.pointBit0(e); break;
            case 5: BgUnit.fetchBit0(e); break;
            case 6: BgUnit.pointBit1(e); break;
            case 7: BgUnit.fetchBit1(e); BgUnit.synthesize(e); break;
            }
        }

        static void spTick(R2C02State e, int step)
        {
            switch (step)
            {
            case 0: BgUnit.pointName(e); break;
            case 1: BgUnit.fetchName(e); break;
            case 2: BgUnit.pointAttr(e); break;
            case 3: BgUnit.fetchAttr(e); break;
            case 4: SpUnit.pointBit0(e); break;
            case 5: SpUnit.fetchBit0(e); break;
            case 6: SpUnit.pointBit1(e); break;
            case 7: SpUnit.fetchBit1(e); SpUnit.synthesize(e); break;
            }
        }

        static void scrollTick(R2C02State e)
        {
            if ((e.h & 0x107) == 0x007 ||
                (e.h & 0x1f7) == 0x147)
            {
                clockX(e);
            }

            if (e.h == 255) { clockY(e); }
            if (e.h == 256) { resetX(e); }

            if (e.v == 261 && (e.h >= 280 && e.h <= 304))
            {
                resetY(e);
            }
        }

        static void forcedBlankCycle(R2C02State e)
        {
            if (e.v >= 240) return;
            if (e.h >= 256) return;

            var pixel = forcedBlankPixel(e);
            var color = CGRAM.read(e, pixel);

            raster[e.h] = Palette.Lookup[(color & e.clipping) | e.emphasis];
        }

        static int forcedBlankPixel(R2C02State e)
        {
            return (e.scroll_address & 0x3f00) == 0x3f00
                ? e.scroll_address
                : 0
                ;
        }

        static void renderPixel(R2C02State e)
        {
            var pixel = colorMultiplexer(e);
            var color = CGRAM.read(e, pixel);

            raster[e.h] = Palette.Lookup[(color & e.clipping) | e.emphasis];
        }

        static int colorMultiplexer(R2C02State e)
        {
            int bkg = BgUnit.getPixel(e);
            int obj = SpUnit.getPixel(e);

            if ((bkg & 3) == 0) { return obj; }
            if ((obj & 3) == 0) { return bkg; }

            if ((obj & 0x4000) != 0)
            {
                e.obj_zero_hit = true;
            }

            return (obj & 0x8000) != 0
                ? bkg
                : obj
                ;
        }

        static void tickCounter(R2C02State e)
        {
            if (e.v == 240 && e.h == 340) { e.vbl_hold = 1; }
            if (e.v == 241 && e.h == 0) { e.vbl_flag = e.vbl_hold; }
            //  if (e.v == 241 && e.h ==   2) { VBL(); }

            if (e.v == 260 && e.h == 340)
            {
                e.obj_overflow = false;
                e.obj_zero_hit = false;
            }

            if (e.v == 260 && e.h == 340) { e.vbl_hold = 0; }
            if (e.v == 261 && e.h == 0) { e.vbl_flag = e.vbl_hold; }
            //  if (e.v == 261 && e.h ==   2) { VBL(); }

            e.h++;
        }

        static void clockX(R2C02State e)
        {
            if ((e.scroll_address & 0x001f) == 0x001f)
            {
                e.scroll_address ^= 0x041f;
            }
            else
            {
                e.scroll_address += 0x0001;
            }
        }

        static void clockY(R2C02State e)
        {
            if ((e.scroll_address & 0x7000) == 0x7000)
            {
                e.scroll_address ^= 0x7000;

                switch (e.scroll_address & 0x3e0)
                {
                case 0x3a0: e.scroll_address ^= 0xba0; break;
                case 0x3e0: e.scroll_address ^= 0x3e0; break;

                default:
                    e.scroll_address += 0x20;
                    break;
                }
            }
            else
            {
                e.scroll_address += 0x1000;
            }
        }

        static void resetX(R2C02State e)
        {
            e.scroll_address &= 0x7be0;
            e.scroll_address |= e.scroll_temp & 0x041f;
        }

        static void resetY(R2C02State e)
        {
            e.scroll_address &= 0x041f;
            e.scroll_address |= e.scroll_temp & 0x7be0;
        }

        public static void init(R2C02State e, IVideoBackend video)
        {
            raster = video.GetRaster(0);
        }

        public static void tick(R2C02State e, IVideoBackend video)
        {
            if (e.bkg_enabled || e.obj_enabled)
            {
                if (e.v < 240) { activeCycle(e); }
                if (e.v == 261) { bufferCycle(e); }
            }
            else
            {
                forcedBlankCycle(e);
            }

            tickCounter(e);

            if (e.h == 341)
            {
                e.h = 0;
                e.v++;

                if (e.v == 261)
                {
                    e.field ^= true;
                }

                if (e.v == 262)
                {
                    e.v = 0;

                    video.Render();
                }

                if (e.v < 240)
                {
                    raster = video.GetRaster(e.v);
                }
            }
        }
    }
}
