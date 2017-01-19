using Beta.Platform.Messaging;
using Beta.Platform.Video;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.PPU
{
    public sealed partial class Ppu
    {
        private static int[][][] priorityLut = new[]
        {
            new[] { new[] { 8, 11 }, new[] { 7, 10 }, new[] { 2,  5 }, new[] { 1, 4 }, new[] { 3, 6, 9, 12 } }, // mode 0
            new[] { new[] { 6,  9 }, new[] { 5,  8 }, new[] { 1,  3 }, new[] { 0, 0 }, new[] { 2, 4, 7, 10 } }, // mode 1
            new[] { new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 0, 0, 0,  0 } },
            new[] { new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 0, 0, 0,  0 } },
            new[] { new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 0, 0, 0,  0 } },
            new[] { new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 0, 0, 0,  0 } },
            new[] { new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 0, 0, 0,  0 } },
            new[] { new[] { 2,  3 }, new[] { 0,  0 }, new[] { 0,  0 }, new[] { 0, 0 }, new[] { 1, 4, 5,  6 } },
            new[] { new[] { 5,  8 }, new[] { 4,  7 }, new[] { 1, 10 }, new[] { 0, 0 }, new[] { 2, 3, 6,  9 } }  // mode 1 priority
        };

        private readonly SPpuState sppu;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<HBlankSignal> hblank;
        private readonly IProducer<VBlankSignal> vblank;
        private readonly IVideoBackend video;

        private bool forceBlank;
        private bool interlace;
        private bool overscan;
        private bool pseudoHi;
        private bool[] mathEnable = new bool[6];
        private byte ppu1Open;
        private byte ppu2Open;
        private byte ppu1Stat = 1;
        private byte ppu2Stat = 2;
        private int forceMainToBlack;
        private int fixedColor;
        private int brightness;
        private bool hlatch_toggle;
        private bool vlatch_toggle;
        private int hlatch;
        private int vlatch;
        private int hclock;
        private int vclock;
        private int[] colors = colorLookup[0];
        private int[] raster;
        private int colorMathEnabled;
        private int mathType;
        private int product;
        private int cycles;

        static Ppu()
        {
            for (var brightness = 0; brightness < 16; brightness++)
            {
                for (var colour = 0; colour < 32768; colour++)
                {
                    var r = (colour << 3) & 0xf8;
                    var g = (colour >> 2) & 0xf8;
                    var b = (colour >> 7) & 0xf8;

                    // apply gradient to lower bits (this will make black=$000000 and white=$ffffff)
                    r |= (r >> 5);
                    g |= (r >> 5);
                    b |= (r >> 5);

                    r = (r * brightness) / 15;
                    g = (g * brightness) / 15;
                    b = (b * brightness) / 15;

                    colorLookup[brightness][colour] = (r << 16) | (g << 8) | b;
                }
            }
        }

        public Ppu(
            State state,
            IVideoBackend video,
            IProducer<FrameSignal> frame,
            IProducer<HBlankSignal> hblank,
            IProducer<VBlankSignal> vblank)
        {
            this.sppu = state.sppu;
            this.video = video;
            this.frame = frame;
            this.hblank = hblank;
            this.vblank = vblank;

            bg0 = new Background(sppu.bg0, this);
            bg1 = new Background(sppu.bg1, this);
            bg2 = new Background(sppu.bg2, this);
            bg3 = new Background(sppu.bg3, this);
            clr = new ColorGeneration(this);
            spr = new Sprite(this);

            raster = new int[256];
        }

        public byte Peek2134()
        {
            product = (short)sppu.m7.a * ((sbyte)(sppu.m7.b >> 8));

            return ppu1Open = ((byte)(product >> 0));
        }

        public byte Peek2135()
        {
            product = (short)sppu.m7.a * ((sbyte)(sppu.m7.b >> 8));

            return ppu1Open = ((byte)(product >> 8));
        }

        public byte Peek2136()
        {
            product = (short)sppu.m7.a * ((sbyte)(sppu.m7.b >> 8));

            return ppu1Open = ((byte)(product >> 16));
        }

        public byte Peek2137()
        {
            hlatch = hclock;
            vlatch = vclock;

            return 0;
        }

        public byte Peek213C()
        {
            hlatch_toggle = !hlatch_toggle;

            ppu2Open = hlatch_toggle
                ? (byte)((ppu2Open & 0x00) | (hlatch >> 0))
                : (byte)((ppu2Open & 0xfe) | (hlatch >> 8))
                ;

            return ppu2Open;
        }

        public byte Peek213D()
        {
            vlatch_toggle = !vlatch_toggle;

            ppu2Open = vlatch_toggle
                ? (byte)((ppu2Open & 0x00) | (vlatch >> 0))
                : (byte)((ppu2Open & 0xfe) | (vlatch >> 8))
                ;

            return ppu2Open;
        }

        public byte Peek213E()
        {
            return ppu1Stat;
        }

        public byte Peek213F()
        {
            var data = ppu2Stat;

            hlatch_toggle = false;
            vlatch_toggle = false;

            return data;
        }

        public void Poke2100(byte data)
        {
            forceBlank = (data & 0x80) != 0;
            brightness = (data & 0x0f);

            colors = colorLookup[brightness];
        }

        public void Poke2101(byte data)
        {
            spr.Addr = (data & 0x07) << 13;
            spr.Name = (data & 0x18) << 9;
            spr.Name += 0x1000;
            spr.Width = Sprite.WidthLut[(data & 0xe0) >> 5];
            spr.Height = Sprite.HeightLut[(data & 0xe0) >> 5];
        }

        public void Poke2105(byte data)
        {
            sppu.bg_mode = (data & 0x07);
            sppu.bg_priority = (data & 0x08) != 0;
            sppu.bg0.char_size = (data & 0x10) != 0 ? 16 : 8;
            sppu.bg1.char_size = (data & 0x20) != 0 ? 16 : 8;
            sppu.bg2.char_size = (data & 0x40) != 0 ? 16 : 8;
            sppu.bg3.char_size = (data & 0x80) != 0 ? 16 : 8;

            var table = priorityLut[sppu.bg_mode];

            if (sppu.bg_mode == 1 && sppu.bg_priority)
                table = priorityLut[8];

            bg0.priorities = table[0];
            bg1.priorities = table[1];
            bg2.priorities = table[2];
            bg3.priorities = table[3];
            spr.priorities = table[4];
        }

        public void Poke2106(byte data)
        {
            sppu.bg0.mosaic = (data & 0x01) != 0;
            sppu.bg1.mosaic = (data & 0x02) != 0;
            sppu.bg2.mosaic = (data & 0x04) != 0;
            sppu.bg3.mosaic = (data & 0x08) != 0;

            sppu.bg_mosaic_size = (data & 0xf0) >> 4;
        }

        public void Poke2107(byte data)
        {
            sppu.bg0.name_size = (data & 0x03);
            sppu.bg0.name_base = (data & 0x7c) << 8;
        }

        public void Poke2108(byte data)
        {
            sppu.bg1.name_size = (data & 0x03);
            sppu.bg1.name_base = (data & 0x7c) << 8;
        }

        public void Poke2109(byte data)
        {
            sppu.bg2.name_size = (data & 0x03);
            sppu.bg2.name_base = (data & 0x7c) << 8;
        }

        public void Poke210A(byte data)
        {
            sppu.bg3.name_size = (data & 0x03);
            sppu.bg3.name_base = (data & 0x7c) << 8;
        }

        public void Poke210B(byte data)
        {
            sppu.bg0.char_base = (data & 0x07) << 12;
            sppu.bg1.char_base = (data & 0x70) << 8;
        }

        public void Poke210C(byte data)
        {
            sppu.bg2.char_base = (data & 0x07) << 12;
            sppu.bg3.char_base = (data & 0x70) << 8;
        }

        public void Poke210D(byte data)
        {
            WriteHOffset(sppu.bg0, data);

            sppu.m7.h_offset = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke210E(byte data)
        {
            WriteVOffset(sppu.bg0, data);

            sppu.m7.v_offset = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke210F(byte data)
        {
            WriteHOffset(sppu.bg1, data);
        }

        public void Poke2110(byte data)
        {
            WriteVOffset(sppu.bg1, data);
        }

        public void Poke2111(byte data)
        {
            WriteHOffset(sppu.bg2, data);
        }

        public void Poke2112(byte data)
        {
            WriteVOffset(sppu.bg2, data);
        }

        public void Poke2113(byte data)
        {
            WriteHOffset(sppu.bg3, data);
        }

        public void Poke2114(byte data)
        {
            WriteVOffset(sppu.bg3, data);
        }

        public void Poke2115(byte data)
        {
            vram_control = data;

            switch (vram_control & 3)
            {
            case 0: vram_step = 0x01; break;
            case 1: vram_step = 0x20; break;
            case 2: vram_step = 0x80; break;
            case 3: vram_step = 0x80; break;
            }
        }

        public void Poke211A(byte data)
        {
            sppu.m7.control = data;
        }

        public void Poke211B(byte data)
        {
            sppu.m7.a = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke211C(byte data)
        {
            sppu.m7.b = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke211D(byte data)
        {
            sppu.m7.c = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke211E(byte data)
        {
            sppu.m7.d = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke211F(byte data)
        {
            sppu.m7.x = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke2120(byte data)
        {
            sppu.m7.y = (ushort)((data << 8) | sppu.m7.latch);
            sppu.m7.latch = data;
        }

        public void Poke2123(byte data)
        {
            bg0.window_1_inverted = (data & 0x01) != 0;
            bg0.window_1_enable   = (data & 0x02) != 0;
            bg0.window_2_inverted = (data & 0x04) != 0;
            bg0.window_2_enable   = (data & 0x08) != 0;

            bg1.window_1_inverted = (data & 0x10) != 0;
            bg1.window_1_enable   = (data & 0x20) != 0;
            bg1.window_2_inverted = (data & 0x40) != 0;
            bg1.window_2_enable   = (data & 0x80) != 0;
        }

        public void Poke2124(byte data)
        {
            bg2.window_1_inverted = (data & 0x01) != 0;
            bg2.window_1_enable   = (data & 0x02) != 0;
            bg2.window_2_inverted = (data & 0x04) != 0;
            bg2.window_2_enable   = (data & 0x08) != 0;

            bg3.window_1_inverted = (data & 0x10) != 0;
            bg3.window_1_enable   = (data & 0x20) != 0;
            bg3.window_2_inverted = (data & 0x40) != 0;
            bg3.window_2_enable   = (data & 0x80) != 0;
        }

        public void Poke2125(byte data)
        {
            spr.window_1_inverted = (data & 0x01) != 0;
            spr.window_1_enable   = (data & 0x02) != 0;
            spr.window_2_inverted = (data & 0x04) != 0;
            spr.window_2_enable   = (data & 0x08) != 0;

            clr.window_1_inverted = (data & 0x10) != 0;
            clr.window_1_enable   = (data & 0x20) != 0;
            clr.window_2_inverted = (data & 0x40) != 0;
            clr.window_2_enable   = (data & 0x80) != 0;
        }

        public void Poke2126(byte data)
        {
            sppu.window1.x1 = data;
        }

        public void Poke2127(byte data)
        {
            sppu.window1.x2 = data;
        }

        public void Poke2128(byte data)
        {
            sppu.window2.x1 = data;
        }

        public void Poke2129(byte data)
        {
            sppu.window2.x2 = data;
        }

        public void Poke212A(byte data)
        {
            bg0.window_logic = (data >> 0) & 3;
            bg1.window_logic = (data >> 2) & 3;
            bg2.window_logic = (data >> 4) & 3;
            bg3.window_logic = (data >> 6) & 3;
        }

        public void Poke212B(byte data)
        {
            spr.window_logic = (data >> 0) & 3;
            clr.window_logic = (data >> 2) & 3;
        }

        public void Poke212C(byte data)
        {
            bg0.screen_main = (data & 0x01) != 0 ? ~0 : 0;
            bg1.screen_main = (data & 0x02) != 0 ? ~0 : 0;
            bg2.screen_main = (data & 0x04) != 0 ? ~0 : 0;
            bg3.screen_main = (data & 0x08) != 0 ? ~0 : 0;
            spr.screen_main = (data & 0x10) != 0 ? ~0 : 0;
        }

        public void Poke212D(byte data)
        {
            bg0.screen_sub = (data & 0x01) != 0 ? ~0 : 0;
            bg1.screen_sub = (data & 0x02) != 0 ? ~0 : 0;
            bg2.screen_sub = (data & 0x04) != 0 ? ~0 : 0;
            bg3.screen_sub = (data & 0x08) != 0 ? ~0 : 0;
            spr.screen_sub = (data & 0x10) != 0 ? ~0 : 0;
        }

        public void Poke212E(byte data)
        {
            bg0.window_main = (data & 0x01) != 0;
            bg1.window_main = (data & 0x02) != 0;
            bg2.window_main = (data & 0x04) != 0;
            bg3.window_main = (data & 0x08) != 0;
            spr.window_main = (data & 0x10) != 0;
        }

        public void Poke212F(byte data)
        {
            bg0.window_sub = (data & 0x01) != 0;
            bg1.window_sub = (data & 0x02) != 0;
            bg2.window_sub = (data & 0x04) != 0;
            bg3.window_sub = (data & 0x08) != 0;
            spr.window_sub = (data & 0x10) != 0;
        }

        public void Poke2130(byte data)
        {
            forceMainToBlack = (data & 0xc0) >> 6;
            colorMathEnabled = (data & 0x30) >> 4;
        }

        public void Poke2131(byte data)
        {
            mathEnable[0] = (data & 0x01) != 0;
            mathEnable[1] = (data & 0x02) != 0;
            mathEnable[2] = (data & 0x04) != 0;
            mathEnable[3] = (data & 0x08) != 0;
            mathEnable[4] = (data & 0x10) != 0;
            mathEnable[5] = (data & 0x20) != 0;
            mathType = (data >> 6) & 3;
        }

        public void Poke2132(byte data)
        {
            if ((data & 0x80) != 0) { fixedColor = (fixedColor & ~0x7c00) | ((data & 0x1f) << 10); }
            if ((data & 0x40) != 0) { fixedColor = (fixedColor & ~0x03e0) | ((data & 0x1f) << 5); }
            if ((data & 0x20) != 0) { fixedColor = (fixedColor & ~0x001f) | ((data & 0x1f) << 0); }
        }

        public void Poke2133(byte data)
        {
            // todo: mode7 extbg bit
            pseudoHi = (data & 0x08) != 0;
            overscan = (data & 0x04) != 0;
            spr.Interlace = (data & 0x02) != 0;
            interlace = (data & 0x01) != 0;
        }

        public void WriteHOffset(BackgroundState bg, byte data)
        {
            bg.h_offset = (data << 8) | (sppu.bg_offset_latch & ~7) | ((bg.h_offset >> 8) & 7);
            sppu.bg_offset_latch = data;
        }

        public void WriteVOffset(BackgroundState bg, byte data)
        {
            bg.v_offset = (data << 8) | sppu.bg_offset_latch;
            sppu.bg_offset_latch = data;
        }

        public void Update()
        {
            hclock++;

            if (hclock == 274) { RenderScanline(); }
            if (hclock == 341)
            {
                hclock = 0;
                vclock++;

                if (vclock == (overscan ? 241 : 225))
                {
                    ppu2Stat ^= 0x80; // toggle field flag every vblank
                    vblank.Produce(new VBlankSignal(true));
                }

                if (vclock == 262)
                {
                    vclock = 0;
                    vblank.Produce(new VBlankSignal(false));

                    ppu1Stat &= 0x3F; // reset time and range flags

                    video.Render();

                    frame.Produce(new FrameSignal());
                }

                if (vclock < 240)
                {
                    raster = video.GetRaster(vclock);
                }
            }

            if (vclock < (overscan ? 241 : 225))
            {
                if (hclock <= 18)
                {
                    hblank.Produce(new HBlankSignal(true));
                }

                if (hclock >= 289)
                {
                    hblank.Produce(new HBlankSignal(false));
                }
            }
        }

        private void RenderScanline()
        {
            for (var i = 0; i < 256; i++)
            {
                bg0.enable[i] = false;
                bg1.enable[i] = false;
                bg2.enable[i] = false;
                bg3.enable[i] = false;
                spr.enable[i] = false;
                raster[i] = 0;
            }

            if (forceBlank || vclock > (overscan ? 0xef : 0xdf))
            {
                return;
            }

            switch (sppu.bg_mode)
            {
            case 0: RenderMode0(); break;
            case 1: RenderMode1(); break;
            case 2: RenderMode2(); break;
            case 3: RenderMode3(); break;
            case 4: RenderMode4(); break;
            case 5: RenderMode5(); break;
            case 6: RenderMode6(); break;
            case 7: RenderMode7(); break; // affine render
            }
        }

        public void Consume(ClockSignal e)
        {
            for (cycles += e.Cycles; cycles >= 4; cycles -= 4)
            {
                Update();
            }
        }
    }
}
