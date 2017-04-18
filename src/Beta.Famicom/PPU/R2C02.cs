using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02
    {
        private readonly R2C02State state;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<VblSignal> vbl;
        private readonly IVideoBackend video;

        private int cycles;
        private int[] raster;

        public R2C02(
            State state,
            IProducer<FrameSignal> frame,
            IProducer<VblSignal> vbl,
            IVideoBackend video)
        {
            this.state = state.r2c02;
            this.frame = frame;
            this.vbl = vbl;
            this.video = video;

            raster = video.GetRaster(0);

            SpUnit.EvaluationReset(this.state);
            SpUnit.InitializeSprite(this.state);
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
            case 0: SpUnit.Evaluation0(state); break;
            case 1: SpUnit.Evaluation1(state); break;
            }

            if (state.h == 0x3f) SpUnit.EvaluationBegin(state);
            if (state.h == 0xff) SpUnit.EvaluationReset(state);
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
            case 0: BgUnit.PointName(state); break;
            case 1: BgUnit.FetchName(state); break;
            case 2: BgUnit.PointAttr(state); break;
            case 3: BgUnit.FetchAttr(state); break;
            case 4: BgUnit.PointBit0(state); break;
            case 5: BgUnit.FetchBit0(state); break;
            case 6: BgUnit.PointBit1(state); break;
            case 7: BgUnit.FetchBit1(state); BgUnit.Synthesize(state); break;
            }
        }

        private void SpTick(int step)
        {
            switch (step)
            {
            case 0: BgUnit.PointName(state); break;
            case 1: BgUnit.FetchName(state); break;
            case 2: BgUnit.PointAttr(state); break;
            case 3: BgUnit.FetchAttr(state); break;
            case 4: SpUnit.PointBit0(state); break;
            case 5: SpUnit.FetchBit0(state); break;
            case 6: SpUnit.PointBit1(state); break;
            case 7: SpUnit.FetchBit1(state); SpUnit.Synthesize(state); break;
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
            var color = CGRAM.Read(state, pixel);

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
            var color = CGRAM.Read(state, pixel);

            raster[state.h] = Palette.Lookup[(color & state.clipping) | state.emphasis];
        }

        private int ColorMultiplexer()
        {
            int bkg = BgUnit.GetPixel(state);
            int obj = SpUnit.GetPixel(state);

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
            state.scroll_address |= state.scroll_temp & 0x041f;
        }

        private void ResetY()
        {
            state.scroll_address &= 0x041f;
            state.scroll_address |= state.scroll_temp & 0x7be0;
        }
    }
}
