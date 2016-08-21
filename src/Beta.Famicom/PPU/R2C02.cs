using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02 : IConsumer<ClockSignal>
    {
        private readonly BgUnit bg;
        private readonly SpUnit sp;
        private readonly CGRAM cgram;
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State state;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<VblSignal> vbl;
        private readonly IVideoBackend video;

        private int cycles;
        private int[] raster;

        public R2C02(
            BgUnit bg,
            SpUnit sp,
            CGRAM cgram,
            R2C02MemoryMap memory,
            State state,
            IProducer<FrameSignal> frame,
            IProducer<VblSignal> vbl,
            IVideoBackend video)
        {
            this.bg = bg;
            this.sp = sp;
            this.memory = memory;
            this.cgram = cgram;
            this.state = state.r2c02;
            this.frame = frame;
            this.vbl = vbl;
            this.video = video;

            sp.EvaluationReset();

            raster = video.GetRaster(0);

            sp.InitializeSprite();
        }

        private bool Rendering()
        {
            return (state.bkg_enabled || state.obj_enabled) && state.v < 240;
        }

        private void VBL()
        {
            var signal = state.vbl_flag & state.vbl_enabled;

            vbl.Produce(new VblSignal(signal));
        }

        private void ActiveCycle()
        {
            var h = state.h;

            if ((h & 0x100) == 0x000)
            {
                BgTick(h & 7);
                RenderPixel();
                EvaluationTick();
            }
            else if ((h & 0x1c0) == 0x100) { SpTick(h & 7); }
            else if ((h & 0x1f0) == 0x140) { BgTick(h & 7); }
            else if ((h & 0x1fc) == 0x150) { BgTick(h & 1); }

            ScrollTick();
        }

        private void EvaluationTick()
        {
            switch (state.h & 1)
            {
            case 0: sp.Evaluation0(); break;
            case 1: sp.Evaluation1(); break;
            }

            if (state.h == 0x3f) sp.EvaluationBegin();
            if (state.h == 0xff) sp.EvaluationReset();
        }

        private void BufferCycle()
        {
            var h = state.h;

            if ((h & 0x100) == 0x000) { BgTick(h & 7); }
            else if ((h & 0x1c0) == 0x100) { SpTick(h & 7); }
            else if ((h & 0x1f0) == 0x140) { BgTick(h & 7); }
            else if ((h & 0x1fc) == 0x150) { BgTick(h & 1); }

            ScrollTick();

            if (h == 337 && state.field)
            {
                Tick();
            }
        }

        private void BgTick(int step)
        {
            switch (step)
            {
            case 0: bg.PointName(); break;
            case 1: bg.FetchName(); break;
            case 2: bg.PointAttr(); break;
            case 3: bg.FetchAttr(); break;
            case 4: bg.PointBit0(); break;
            case 5: bg.FetchBit0(); break;
            case 6: bg.PointBit1(); break;
            case 7: bg.FetchBit1(); bg.Synthesize(); break;
            }
        }

        private void SpTick(int step)
        {
            switch (step)
            {
            case 0: bg.PointName(); break;
            case 1: bg.FetchName(); break;
            case 2: bg.PointAttr(); break;
            case 3: bg.FetchAttr(); break;
            case 4: sp.PointBit0(); break;
            case 5: sp.FetchBit0(); break;
            case 6: sp.PointBit1(); break;
            case 7: sp.FetchBit1(); sp.Synthesize(); break;
            }
        }

        private void ScrollTick()
        {
            if ((state.h & 0x107) == 0x007 ||
                (state.h & 0x1f7) == 0x147)
            {
                ClockX();
            }

            if (state.h == 255) { ClockY(); }
            if (state.h == 256) { ResetX(); }

            if (state.v == 261 && (state.h >= 280 && state.h <= 304))
            {
                ResetY();
            }
        }

        private void ForcedBlankCycle()
        {
            if (state.v >= 240) return;
            if (state.h >= 256) return;

            var pixel = ForcedBlankPixel();
            var color = cgram.Read(pixel);

            raster[state.h] = Palette.Lookup[(color & state.clipping) | state.emphasis];
        }

        private int ForcedBlankPixel()
        {
            return (state.scroll_address & 0x3f00) == 0x3f00
                ? state.scroll_address
                : 0
                ;
        }

        private void RenderPixel()
        {
            var pixel = ColorMultiplexer();
            var color = cgram.Read(pixel);

            raster[state.h] = Palette.Lookup[(color & state.clipping) | state.emphasis];
        }

        private int ColorMultiplexer()
        {
            int bkg = bg.GetPixel();
            int obj = sp.GetPixel();

            if ((bkg & 3) == 0) { return obj; }
            if ((obj & 3) == 0) { return bkg; }

            if ((obj & 0x4000) != 0)
            {
                state.obj_zero_hit = true;
            }

            return (obj & 0x8000) != 0
                ? bkg
                : obj
                ;
        }

        private void Tick()
        {
            if (state.v == 240 && state.h == 340) { state.vbl_hold = 1; }
            if (state.v == 241 && state.h ==   0) { state.vbl_flag = state.vbl_hold; }
            if (state.v == 241 && state.h ==   2) { VBL(); }

            if (state.v == 260 && state.h == 340)
            {
                state.obj_overflow = false;
                state.obj_zero_hit = false;
            }

            if (state.v == 260 && state.h == 340) { state.vbl_hold = 0; }
            if (state.v == 261 && state.h ==   0) { state.vbl_flag = state.vbl_hold; }
            if (state.v == 261 && state.h ==   2) { VBL(); }

            state.h++;
        }

        public void Consume(ClockSignal e)
        {
            const int single = 44;

            for (cycles += e.Cycles; cycles >= single; cycles -= single)
            {
                Update();
            }
        }

        private void Update()
        {
            if (state.bkg_enabled || state.obj_enabled)
            {
                if (state.v <  240) { ActiveCycle(); }
                if (state.v == 261) { BufferCycle(); }
            }
            else
            {
                ForcedBlankCycle();
            }

            Tick();

            if (state.h == 341)
            {
                state.h = 0;
                state.v++;

                if (state.v == 261)
                {
                    state.field ^= true;
                }

                if (state.v == 262)
                {
                    state.v = 0;

                    frame.Produce(null);

                    video.Render();
                }

                if (state.v < 240)
                {
                    raster = video.GetRaster(state.v);
                }
            }
        }

        private void ClockX()
        {
            if ((state.scroll_address & 0x001f) == 0x001f)
            {
                state.scroll_address ^= 0x041f;
            }
            else
            {
                state.scroll_address += 0x0001;
            }
        }

        private void ClockY()
        {
            if ((state.scroll_address & 0x7000) == 0x7000)
            {
                state.scroll_address ^= 0x7000;

                switch (state.scroll_address & 0x3e0)
                {
                case 0x3a0: state.scroll_address ^= 0xba0; break;
                case 0x3e0: state.scroll_address ^= 0x3e0; break;

                default:
                    state.scroll_address += 0x20;
                    break;
                }
            }
            else
            {
                state.scroll_address += 0x1000;
            }
        }

        private void ResetX()
        {
            state.scroll_address &= 0x7be0;
            state.scroll_address |= (ushort)(state.scroll_temp & 0x041f);
        }

        private void ResetY()
        {
            state.scroll_address &= 0x041f;
            state.scroll_address |= (ushort)(state.scroll_temp & 0x7be0);
        }
    }
}
