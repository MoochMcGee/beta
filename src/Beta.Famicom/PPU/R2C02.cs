using Beta.Famicom.CPU;
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

        private int[] bkg = new int[256 + 16];
        private int[] spr = new int[256];
        private int[] raster;

        private int cycles;

        public R2C02(
            CGRAM cgram,
            R2A03Bus r2a03,
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

            r2a03.Map("001- ---- ---- -000", writer: Write2000);
            r2a03.Map("001- ---- ---- -001", writer: Write2001);
            r2a03.Map("001- ---- ---- -010", reader: Read2002);
            r2a03.Map("001- ---- ---- -011", writer: Write2003);
            r2a03.Map("001- ---- ---- -100", reader: Read2004, writer: Write2004);
            r2a03.Map("001- ---- ---- -101", writer: Write2005);
            r2a03.Map("001- ---- ---- -110", writer: Write2006);
            r2a03.Map("001- ---- ---- -111", reader: Read2007, writer: Write2007);

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
                bkg[offset + i] =
                    ((state.fetch_attr << 2) & 12) |
                    ((state.fetch_bit0 >> 7) &  1) |
                    ((state.fetch_bit1 >> 6) &  2);

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        #endregion

        #region Sprite

        private Sprite[] sprFound = new Sprite[8];
        private byte sprLatch;
        private int sprCount;
        private int sprIndex;
        private int sprPhase;

        private void SynthesizeSp()
        {
            if (state.v == 261)
            {
                return;
            }

            var sprite = sprFound[(state.h >> 3) & 7];
            int offset = sprite.X;

            for (var i = 0; i < 8 && offset < 256; i++, offset++)
            {
                var color =
                    ((state.fetch_bit0 >> 7) & 1) |
                    ((state.fetch_bit1 >> 6) & 2);

                if ((spr[offset] & 3) == 0 && color != 0)
                {
                    spr[offset] = 0x10 | ((sprite.Attr << 10) & 0xc000) | ((sprite.Attr << 2) & 12) | color;
                }

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        private void SpriteEvaluation0()
        {
            sprLatch = (byte)(state.h < 64 ? 0xff : state.oam[state.oam_address]);
        }

        private void SpriteEvaluation1()
        {
            if (state.h < 64)
            {
                switch ((state.h >> 1) & 3)
                {
                case 0: sprFound[(state.h >> 3) & 7].Y    = sprLatch; break;
                case 1: sprFound[(state.h >> 3) & 7].Name = sprLatch; break;
                case 2: sprFound[(state.h >> 3) & 7].Attr = sprLatch &= 0xe3; break;
                case 3: sprFound[(state.h >> 3) & 7].X    = sprLatch; break;
                }
            }
            else
            {
                switch (sprPhase)
                {
                case 0:
                    {
                        sprCount++;

                        var raster = (state.v - sprLatch) & 0x1ff;
                        if (raster < state.obj_rasters)
                        {
                            state.oam_address++;
                            sprFound[sprIndex].Y = sprLatch;
                            sprPhase++;
                        }
                        else
                        {
                            if (sprCount != 64)
                            {
                                state.oam_address += 4;
                            }
                            else
                            {
                                state.oam_address = 0;
                                sprPhase = 8;
                            }
                        }
                    }
                    break;

                case 1:
                    state.oam_address++;
                    sprFound[sprIndex].Name = sprLatch;
                    sprPhase++;
                    break;

                case 2:
                    state.oam_address++;
                    sprFound[sprIndex].Attr = sprLatch &= 0xe3;
                    sprPhase++;

                    if (sprCount == 1)
                    {
                        sprFound[sprIndex].Attr |= Sprite.SPR_ZERO;
                    }
                    break;

                case 3:
                    sprFound[sprIndex].X = sprLatch;
                    sprIndex++;

                    if (sprCount != 64)
                    {
                        sprPhase = (sprIndex != 8 ? 0 : 4);
                        state.oam_address++;
                    }
                    else
                    {
                        sprPhase = 8;
                        state.oam_address = 0;
                    }
                    break;

                case 4:
                    {
                        var raster = (state.v - sprLatch) & 0x1ff;
                        if (raster < state.obj_rasters)
                        {
                            state.obj_overflow = true;
                            sprPhase++;
                            state.oam_address++;
                        }
                        else
                        {
                            state.oam_address = (byte)(((state.oam_address + 4) & ~3) + ((state.oam_address + 1) & 3));

                            if (state.oam_address <= 5)
                            {
                                sprPhase = 8;
                                state.oam_address &= 0xfc;
                            }
                        }
                    }
                    break;

                case 5:
                    sprPhase = 6;
                    state.oam_address++;
                    break;

                case 6:
                    sprPhase = 7;
                    state.oam_address++;
                    break;

                case 7:
                    sprPhase = 8;
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

            sprCount = 0;
            sprIndex = 0;
            sprPhase = 0;
        }

        private void EvaluationReset()
        {
            EvaluationBegin();

            for (var i = 0; i < 0x100; i++)
            {
                spr[i] = 0;
            }
        }

        private void PointSpBit0()
        {
            var sprite = sprFound[(state.h >> 3) & 7];
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
            var sprite = sprFound[(state.h >> 3) & 7];

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
            var sprite = sprFound[(state.h >> 3) & 7];

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
                sprFound[i] = new Sprite();
            }

            sprLatch = 0;
            sprCount = 0;
            sprIndex = 0;
            sprPhase = 0;
        }

        private void ResetSprite()
        {
            sprLatch = 0;
            sprCount = 0;
            sprIndex = 0;
            sprPhase = 0;

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

        private void Read2002(ushort address, ref byte data)
        {
            data &= 0x1f;

            if (state.vbl_flag > 0) data |= 0x80;
            if (state.obj_zero_hit) data |= 0x40;
            if (state.obj_overflow) data |= 0x20;

            state.vbl_hold = 0;
            state.vbl_flag = 0;
            state.scroll_swap = false;
            VBL();
        }

        private void Read2004(ushort address, ref byte data)
        {
            if (Rendering())
            {
                data = sprLatch;
            }
            else
            {
                data = state.oam[state.oam_address];
            }
        }

        private void Read2007(ushort address, ref byte data)
        {
            if ((state.scroll_address & 0x3f00) == 0x3f00)
            {
                data = cgram.Read(state.scroll_address);
            }
            else
            {
                data = state.chr;
            }

            memory.Read(state.scroll_address, ref state.chr);

            if (Rendering())
            {
                ClockY();
            }
            else
            {
                state.scroll_address += state.scroll_step;
                state.scroll_address &= 0x7fff;
            }
        }

        private void Write2000(ushort address, byte data)
        {
            state.scroll_temp = (ushort)((state.scroll_temp & 0x73ff) | ((data << 10) & 0x0c00));
            state.scroll_step = (ushort)((data & 0x04) != 0 ? 0x0020 : 0x0001);
            state.obj_address = (ushort)((data & 0x08) != 0 ? 0x1000 : 0x0000);
            state.bkg_address = (ushort)((data & 0x10) != 0 ? 0x1000 : 0x0000);
            state.obj_rasters = (data & 0x20) != 0 ? 0x0010 : 0x0008;
            state.vbl_enabled = (data & 0x80) >> 7;

            VBL();
        }

        private void Write2001(ushort address, byte data)
        {
            state.bkg_clipped = (data & 0x02) == 0;
            state.obj_clipped = (data & 0x04) == 0;
            state.bkg_enabled = (data & 0x08) != 0;
            state.obj_enabled = (data & 0x10) != 0;

            state.clipping = (data & 0x01) != 0 ? 0x30 : 0x3f;
            state.emphasis = (data & 0xe0) << 1;
        }

        private void Write2003(ushort address, byte data)
        {
            state.oam_address = data;
        }

        private void Write2004(ushort address, byte data)
        {
            if ((state.oam_address & 3) == 2)
                data &= 0xe3;

            state.oam[state.oam_address++] = data;
        }

        private void Write2005(ushort address, byte data)
        {
            if (state.scroll_swap = !state.scroll_swap)
            {
                state.scroll_temp = (ushort)((state.scroll_temp & ~0x001f) | ((data & ~7) >> 3));
                state.scroll_fine = (data & 0x07);
            }
            else
            {
                state.scroll_temp = (ushort)((state.scroll_temp & ~0x73e0) | ((data & 7) << 12) | ((data & ~7) << 2));
            }
        }

        private void Write2006(ushort address, byte data)
        {
            if (state.scroll_swap = !state.scroll_swap)
            {
                state.scroll_temp = (ushort)((state.scroll_temp & ~0xff00) | ((data & 0x3f) << 8));
            }
            else
            {
                state.scroll_temp = (ushort)((state.scroll_temp & ~0x00ff) | ((data & 0xff) << 0));
                state.scroll_address = state.scroll_temp;

                Read(address);
            }
        }

        private void Write2007(ushort address, byte data)
        {
            if ((state.scroll_address & 0x3f00) == 0x3f00)
            {
                cgram.Write(state.scroll_address, data);
            }
            else
            {
                memory.Write(state.scroll_address, data);
            }

            if (Rendering())
            {
                ClockY();
            }
            else
            {
                state.scroll_address += state.scroll_step;
                state.scroll_address &= 0x7fff;
            }
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
                BkgTick(h & 7);
                RenderPixel();
                EvaluationTick();
            }
            else if ((h & 0x1c0) == 0x100) { ObjTick(h & 7); }
            else if ((h & 0x1f0) == 0x140) { BkgTick(h & 7); }
            else if ((h & 0x1fc) == 0x150) { BkgTick(h & 1); }

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

            if ((h & 0x100) == 0x000) { BkgTick(h & 7); }
            else if ((h & 0x1c0) == 0x100) { ObjTick(h & 7); }
            else if ((h & 0x1f0) == 0x140) { BkgTick(h & 7); }
            else if ((h & 0x1fc) == 0x150) { BkgTick(h & 1); }

            ScrollTick();

            if (h == 337 && state.field)
            {
                Tick();
            }
        }

        private void BkgTick(int step)
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

        private void ObjTick(int step)
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

            raster[state.h] = Palette.Ntsc[(color & state.clipping) | state.emphasis];
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

            raster[state.h] = Palette.Ntsc[(color & state.clipping) | state.emphasis];
        }

        private int ColorMultiplexer()
        {
            int bkg = GetBkgPixel();
            int obj = GetObjPixel();

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

        private int GetBkgPixel()
        {
            if (!state.bkg_enabled || (state.bkg_clipped && state.h < 8))
            {
                return 0;
            }

            return bkg[state.h + state.scroll_fine];
        }

        private int GetObjPixel()
        {
            if (!state.obj_enabled || (state.obj_clipped && state.h < 8) || state.h == 255)
            {
                return 0;
            }

            return spr[state.h];
        }

        private byte Read(int address)
        {
            byte data = 0;
            memory.Read((ushort)address, ref data);
            return data;
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

        public void Update()
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

        public void ClockX()
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

        public void ClockY()
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

        public void ResetX()
        {
            state.scroll_address &= 0x7be0;
            state.scroll_address |= (ushort)(state.scroll_temp & 0x041f);
        }

        public void ResetY()
        {
            state.scroll_address &= 0x041f;
            state.scroll_address |= (ushort)(state.scroll_temp & 0x7be0);
        }
    }
}
