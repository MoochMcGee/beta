using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Platform;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.Famicom.PPU
{
    public sealed class R2C02 : IConsumer<ClockSignal>
    {
        private readonly CGRAM cgram;
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State state;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<VblSignal> vbl;
        private readonly IVideoBackend video;

        private int[] bg = new int[256 + 16];
        private int[] sp = new int[256];
        private int[] raster;

        private int cycles;

        public R2C02(
            CGRAM cgram,
            R2C02MemoryMap memory,
            State state,
            IProducer<FrameSignal> frame,
            IProducer<VblSignal> vbl,
            IVideoBackend video)
        {
            this.memory = memory;
            this.cgram = cgram;
            this.state = state.r2c02;
            this.frame = frame;
            this.vbl = vbl;
            this.video = video;

            EvaluationReset();

            raster = video.GetRaster(0);

            InitializeSprite();
        }

        #region Background

        private void FetchBgName()
        {
            memory.Read(state.fetch_address, ref state.fetch_name);
        }

        private void FetchBgAttr()
        {
            memory.Read(state.fetch_address, ref state.fetch_attr);

            var x = (state.scroll_address >> 0) & 2;
            var y = (state.scroll_address >> 5) & 2;
            var shift = (y << 1) | x;

            state.fetch_attr = (byte)(state.fetch_attr >> shift);
        }

        private void FetchBgBit0()
        {
            memory.Read(state.fetch_address, ref state.fetch_bit0);
        }

        private void FetchBgBit1()
        {
            memory.Read(state.fetch_address, ref state.fetch_bit1);
        }

        private void PointBgName()
        {
            state.fetch_address = (ushort)(0x2000 | (state.scroll_address & 0xfff));
        }

        private void PointBgAttr()
        {
            var x = ((state.scroll_address >> 2) & 7);
            var y = ((state.scroll_address >> 4) & 0x38);

            state.fetch_address = (ushort)(0x23c0 | (state.scroll_address & 0xc00) | y | x);
        }

        private void PointBgBit0()
        {
            var line = (state.scroll_address >> 12) & 7;

            state.fetch_address = (ushort)(state.bkg_address | (state.fetch_name << 4) | 0 | line);
        }

        private void PointBgBit1()
        {
            var line = (state.scroll_address >> 12) & 7;

            state.fetch_address = (ushort)(state.bkg_address | (state.fetch_name << 4) | 8 | line);
        }

        private void SynthesizeBg()
        {
            var offset = (state.h + 9) % 336;

            for (var i = 0; i < 8; i++)
            {
                bg[offset + i] =
                    ((state.fetch_attr << 2) & 12) |
                    ((state.fetch_bit0 >> 7) &  1) |
                    ((state.fetch_bit1 >> 6) &  2);

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        #endregion

        #region Sprite

        private Sprite[] spFound = new Sprite[8];
        private byte spLatch;
        private int spCount;
        private int spIndex;
        private int spPhase;

        private void SynthesizeSp()
        {
            if (state.v == 261)
            {
                return;
            }

            var sprite = spFound[(state.h >> 3) & 7];
            int offset = sprite.X;

            for (var i = 0; i < 8 && offset < 256; i++, offset++)
            {
                var color =
                    ((state.fetch_bit0 >> 7) & 1) |
                    ((state.fetch_bit1 >> 6) & 2);

                if ((sp[offset] & 3) == 0 && color != 0)
                {
                    sp[offset] = 0x10 | ((sprite.Attr << 10) & 0xc000) | ((sprite.Attr << 2) & 12) | color;
                }

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        private void SpriteEvaluation0()
        {
            spLatch = (byte)(state.h < 64 ? 0xff : state.oam[state.oam_address]);
        }

        private void SpriteEvaluation1()
        {
            if (state.h < 64)
            {
                switch ((state.h >> 1) & 3)
                {
                case 0: spFound[(state.h >> 3) & 7].Y    = spLatch; break;
                case 1: spFound[(state.h >> 3) & 7].Name = spLatch; break;
                case 2: spFound[(state.h >> 3) & 7].Attr = spLatch &= 0xe3; break;
                case 3: spFound[(state.h >> 3) & 7].X    = spLatch; break;
                }
            }
            else
            {
                switch (spPhase)
                {
                case 0:
                    {
                        spCount++;

                        var raster = (state.v - spLatch) & 0x1ff;
                        if (raster < state.obj_rasters)
                        {
                            state.oam_address++;
                            spFound[spIndex].Y = spLatch;
                            spPhase++;
                        }
                        else
                        {
                            if (spCount != 64)
                            {
                                state.oam_address += 4;
                            }
                            else
                            {
                                state.oam_address = 0;
                                spPhase = 8;
                            }
                        }
                    }
                    break;

                case 1:
                    state.oam_address++;
                    spFound[spIndex].Name = spLatch;
                    spPhase++;
                    break;

                case 2:
                    state.oam_address++;
                    spFound[spIndex].Attr = spLatch &= 0xe3;
                    spPhase++;

                    if (spCount == 1)
                    {
                        spFound[spIndex].Attr |= Sprite.SPR_ZERO;
                    }
                    break;

                case 3:
                    spFound[spIndex].X = spLatch;
                    spIndex++;

                    if (spCount != 64)
                    {
                        spPhase = (spIndex != 8 ? 0 : 4);
                        state.oam_address++;
                    }
                    else
                    {
                        spPhase = 8;
                        state.oam_address = 0;
                    }
                    break;

                case 4:
                    {
                        var raster = (state.v - spLatch) & 0x1ff;
                        if (raster < state.obj_rasters)
                        {
                            state.obj_overflow = true;
                            spPhase++;
                            state.oam_address++;
                        }
                        else
                        {
                            state.oam_address = (byte)(((state.oam_address + 4) & ~3) + ((state.oam_address + 1) & 3));

                            if (state.oam_address <= 5)
                            {
                                spPhase = 8;
                                state.oam_address &= 0xfc;
                            }
                        }
                    }
                    break;

                case 5:
                    spPhase = 6;
                    state.oam_address++;
                    break;

                case 6:
                    spPhase = 7;
                    state.oam_address++;
                    break;

                case 7:
                    spPhase = 8;
                    state.oam_address++;
                    break;

                case 8:
                    state.oam_address += 4;
                    break;
                }
            }
        }

        private void EvaluationBegin()
        {
            state.oam_address = 0;

            spCount = 0;
            spIndex = 0;
            spPhase = 0;
        }

        private void EvaluationReset()
        {
            EvaluationBegin();

            for (var i = 0; i < 0x100; i++)
            {
                sp[i] = 0;
            }
        }

        private void PointSpBit0()
        {
            var sprite = spFound[(state.h >> 3) & 7];
            var raster = state.v - sprite.Y;

            if ((sprite.Attr & Sprite.V_FLIP) != 0)
                raster ^= 0xf;

            if (state.obj_rasters == 8)
            {
                state.fetch_address = (ushort)((sprite.Name << 4) | (raster & 7) | state.obj_address);
            }
            else
            {
                sprite.Name = (byte)((sprite.Name >> 1) | (sprite.Name << 7));

                state.fetch_address = (ushort)((sprite.Name << 5) | (raster & 7) | (raster << 1 & 0x10));
            }

            state.fetch_address |= 0;
        }

        private void PointSpBit1()
        {
            state.fetch_address |= 8;
        }

        private void FetchSpBit0()
        {
            var sprite = spFound[(state.h >> 3) & 7];

            memory.Read(state.fetch_address, ref state.fetch_bit0);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                state.fetch_bit0 = 0;
            }
            else if ((sprite.Attr & Sprite.H_FLIP) != 0)
            {
                state.fetch_bit0 = Utility.ReverseLookup[state.fetch_bit0];
            }
        }

        private void FetchSpBit1()
        {
            var sprite = spFound[(state.h >> 3) & 7];

            memory.Read(state.fetch_address, ref state.fetch_bit1);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                state.fetch_bit1 = 0;
            }
            else if ((sprite.Attr & Sprite.H_FLIP) != 0)
            {
                state.fetch_bit1 = Utility.ReverseLookup[state.fetch_bit1];
            }
        }

        private void InitializeSprite()
        {
            for (var i = 0; i < 8; i++)
            {
                spFound[i] = new Sprite();
            }

            spLatch = 0;
            spCount = 0;
            spIndex = 0;
            spPhase = 0;
        }

        private void ResetSprite()
        {
            spLatch = 0;
            spCount = 0;
            spIndex = 0;
            spPhase = 0;

            state.oam_address = 0;
        }

        private class Sprite
        {
            public const int V_FLIP = 0x80;
            public const int H_FLIP = 0x40;
            public const int PRIORITY = 0x20;
            public const int SPR_ZERO = 0x10;

            public byte Y = 0xff;
            public byte Name = 0xff;
            public byte Attr = 0xe3;
            public byte X = 0xff;
        }

        #endregion

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
            case 0: SpriteEvaluation0(); break;
            case 1: SpriteEvaluation1(); break;
            }

            if (state.h == 0x3f) EvaluationBegin();
            if (state.h == 0xff) EvaluationReset();
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
            case 0: PointBgName(); break;
            case 1: FetchBgName(); break;
            case 2: PointBgAttr(); break;
            case 3: FetchBgAttr(); break;
            case 4: PointBgBit0(); break;
            case 5: FetchBgBit0(); break;
            case 6: PointBgBit1(); break;
            case 7: FetchBgBit1(); SynthesizeBg(); break;
            }
        }

        private void SpTick(int step)
        {
            switch (step)
            {
            case 0: PointBgName(); break;
            case 1: FetchBgName(); break;
            case 2: PointBgAttr(); break;
            case 3: FetchBgAttr(); break;
            case 4: PointSpBit0(); break;
            case 5: FetchSpBit0(); break;
            case 6: PointSpBit1(); break;
            case 7: FetchSpBit1(); SynthesizeSp(); break;
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
            int bkg = GetBgPixel();
            int obj = GetSpPixel();

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

        private int GetBgPixel()
        {
            if (!state.bkg_enabled || (state.bkg_clipped && state.h < 8))
            {
                return 0;
            }

            return bg[state.h + state.scroll_fine];
        }

        private int GetSpPixel()
        {
            if (!state.obj_enabled || (state.obj_clipped && state.h < 8) || state.h == 255)
            {
                return 0;
            }

            return sp[state.h];
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
