using Beta.Famicom.Abstractions;
using Beta.Famicom.Messaging;
using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.Famicom.PPU
{
    public class R2C02 : Processor, IConsumer<ClockSignal>
    {
        private readonly IBus bus;
        private readonly IProducer<FrameSignal> frameProducer;
        private readonly IProducer<PpuAddressSignal> addressProducer;
        private readonly IProducer<VblNmiSignal> vblNmiProducer;
        private readonly IVideoBackend video;

        private Fetch fetch = new Fetch();
        private Scroll scroll = new Scroll();
        private Synthesizer bkg = new Synthesizer(256 + 16);
        private Synthesizer spr = new Synthesizer(256);
        private bool field;
        private bool sprOverrun;
        private bool sprZerohit;
        private int vblEnabled;
        private int vblFlag;
        private int vblHold;
        private int hclock;
        private int vclock;
        private int clipping;
        private int emphasis;
        private int[] raster;
        private byte chr;
        private byte oamAddress;
        private byte oamAddressLatch;
        private byte[] oam = new byte[256];
        private byte[] pal = new byte[32];

        private bool Rendering
        {
            get { return (bkg.Enabled || spr.Enabled) && vclock < 240; }
        }

        public R2C02(
            R2C02Bus bus,
            IProducer<FrameSignal> frameProducer,
            IProducer<PpuAddressSignal> addressProducer,
            IProducer<VblNmiSignal> vblNmiProducer,
            IVideoBackend video)
        {
            this.bus = bus;
            this.frameProducer = frameProducer;
            this.addressProducer = addressProducer;
            this.vblNmiProducer = vblNmiProducer;
            this.video = video;

            Single = 44;

            EvaluationReset();
        }

        #region Background

        private void FetchBgName()
        {
            fetch.Name = PeekByte(fetch.Address);
        }

        private void FetchBgAttr()
        {
            fetch.Attr = (byte)(PeekByte(fetch.Address) >> ((scroll.Address >> 4 & 4) | (scroll.Address & 2)));
        }

        private void FetchBgBit0()
        {
            fetch.Bit0 = PeekByte(fetch.Address);
        }

        private void FetchBgBit1()
        {
            fetch.Bit1 = PeekByte(fetch.Address);
        }

        private void PointBgName()
        {
            fetch.Address = (ushort)(0x2000 | (scroll.Address & 0xfff));
            AddressUpdate(fetch.Address);
        }

        private void PointBgAttr()
        {
            fetch.Address =
                (ushort)
                    (0x23c0 | (scroll.Address & 0xc00) | ((scroll.Address >> 4) & 0x38) | ((scroll.Address >> 2) & 7));
            AddressUpdate(fetch.Address);
        }

        private void PointBgBit0()
        {
            fetch.Address = (ushort)(bkg.Address | (fetch.Name << 4) | 0 | ((scroll.Address >> 12) & 7));
            AddressUpdate(fetch.Address);
        }

        private void PointBgBit1()
        {
            fetch.Address = (ushort)(bkg.Address | (fetch.Name << 4) | 8 | ((scroll.Address >> 12) & 7));
            AddressUpdate(fetch.Address);
        }

        private void SynthesizeBg()
        {
            var offset = (hclock + 9) % 336;

            for (var i = 0; i < 8; i++, offset++, fetch.Bit0 <<= 1, fetch.Bit1 <<= 1)
            {
                bkg.Pixels[offset] = 0x3f00 | ((fetch.Attr) << 2 & 12) | ((fetch.Bit0 >> 7) & 1) |
                                     ((fetch.Bit1 >> 6) & 2);
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
            if (vclock == 261)
            {
                return;
            }

            var sprite = sprFound[hclock >> 3 & 7];
            int offset = sprite.X;

            for (var i = 0; i < 8 && offset < 256; i++, offset++, fetch.Bit0 <<= 1, fetch.Bit1 <<= 1)
            {
                var color = (fetch.Bit0 >> 7 & 1) | (fetch.Bit1 >> 6 & 2);

                if ((spr.Pixels[offset] & 3) == 0 && color != 0)
                {
                    spr.Pixels[offset] = 0x3f10 | ((sprite.Attr << 10) & 0xc000) | ((sprite.Attr << 2) & 12) | color;
                }
            }
        }

        private void SpriteEvaluation0()
        {
            //if (vclock == 261) {
            //    return;
            //}

            sprLatch = (byte)(hclock < 64 ? 0xff : oam[oamAddress]);
        }

        private void SpriteEvaluation1()
        {
            if (vclock == 261)
                return;

            if (hclock < 64)
            {
                switch (hclock >> 1 & 3)
                {
                case 0:
                    sprFound[(hclock >> 3) & 7].Y = sprLatch;
                    break;

                case 1:
                    sprFound[(hclock >> 3) & 7].Name = sprLatch;
                    break;

                case 2:
                    sprFound[(hclock >> 3) & 7].Attr = sprLatch &= 0xe3;
                    break;

                case 3:
                    sprFound[(hclock >> 3) & 7].X = sprLatch;
                    break;
                }
            }
            else
            {
                switch (sprPhase)
                {
                case 0:
                    {
                        sprCount++;

                        var raster = (vclock - sprLatch) & 0x1ff;

                        if (raster < spr.Rasters)
                        {
                            oamAddress++;
                            sprFound[sprIndex].Y = sprLatch;
                            sprPhase++;
                        }
                        else
                        {
                            if (sprCount != 64)
                            {
                                oamAddress += 4;
                            }
                            else
                            {
                                oamAddress = 0;
                                sprPhase = 8;
                            }
                        }
                    }
                    break;

                case 1:
                    oamAddress++;
                    sprFound[sprIndex].Name = sprLatch;
                    sprPhase++;
                    break;

                case 2:
                    oamAddress++;
                    sprFound[sprIndex].Attr = sprLatch &= 0xe3;
                    sprPhase++;

                    if (sprCount == 1)
                        sprFound[sprIndex].Attr |= Sprite.SPR_ZERO;
                    break;

                case 3:
                    sprFound[sprIndex].X = sprLatch;
                    sprIndex++;

                    if (sprCount != 64)
                    {
                        sprPhase = (sprIndex != 8 ? 0 : 4);
                        oamAddress++;
                    }
                    else
                    {
                        sprPhase = 8;
                        oamAddress = 0;
                    }
                    break;

                case 4:
                    {
                        var raster = (vclock - sprLatch) & 0x1ff;

                        if (raster < spr.Rasters)
                        {
                            sprOverrun = true;
                            sprPhase++;
                            oamAddress++;
                        }
                        else
                        {
                            oamAddress = (byte)(((oamAddress + 4) & ~3) + ((oamAddress + 1) & 3));

                            if (oamAddress <= 5)
                            {
                                sprPhase = 8;
                                oamAddress &= 0xfc;
                            }
                        }
                    }
                    break;

                case 5:
                    sprPhase = 6;
                    oamAddress++;
                    break;

                case 6:
                    sprPhase = 7;
                    oamAddress++;
                    break;

                case 7:
                    sprPhase = 8;
                    oamAddress++;
                    break;

                case 8:
                    oamAddress += 4;
                    break;
                }
            }
        }

        private void EvaluationBegin()
        {
            oamAddress = 0;

            sprCount = 0;
            sprIndex = 0;
            sprPhase = 0;
        }

        private void EvaluationReset()
        {
            EvaluationBegin();

            for (var i = 0; i < 0x100; i++)
            {
                spr.Pixels[i] = 0x3f00;
            }
        }

        private void PointSpBit0()
        {
            var sprite = sprFound[hclock >> 3 & 7];
            var raster = vclock - sprite.Y;

            if ((sprite.Attr & Sprite.V_FLIP) != 0)
                raster ^= 0xf;

            if (spr.Rasters == 8)
            {
                fetch.Address = (ushort)((sprite.Name << 4) | (raster & 7) | spr.Address);
            }
            else
            {
                sprite.Name = (byte)((sprite.Name >> 1) | (sprite.Name << 7));

                fetch.Address = (ushort)((sprite.Name << 5) | (raster & 7) | (raster << 1 & 0x10));
            }

            fetch.Address |= 0;
            AddressUpdate(fetch.Address);
        }

        private void PointSpBit1()
        {
            fetch.Address |= 8;
            AddressUpdate(fetch.Address);
        }

        private void FetchSpBit0()
        {
            var sprite = sprFound[hclock >> 3 & 7];

            fetch.Bit0 = PeekByte(fetch.Address);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                fetch.Bit0 = 0;
            }
            else if ((sprite.Attr & Sprite.H_FLIP) != 0)
            {
                fetch.Bit0 = Utility.ReverseLookup[fetch.Bit0];
            }
        }

        private void FetchSpBit1()
        {
            var sprite = sprFound[hclock >> 3 & 7];

            fetch.Bit1 = PeekByte(fetch.Address);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                fetch.Bit1 = 0;
            }
            else if ((sprite.Attr & Sprite.H_FLIP) != 0)
            {
                fetch.Bit1 = Utility.ReverseLookup[fetch.Bit1];
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

            oamAddress = 0;
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

            if (vblFlag != 0) data |= 0x80;
            if (sprZerohit) data |= 0x40;
            if (sprOverrun) data |= 0x20;

            vblHold = 0;
            vblFlag = 0;
            scroll.Swap = false;
            VBL();
        }

        private void Read2004(ushort address, ref byte data)
        {
            if ((bkg.Enabled || spr.Enabled) && vclock < 240)
            {
                data = sprLatch;
            }
            else
            {
                data = oam[oamAddress];
            }
        }

        private void Read2007(ushort address, ref byte data)
        {
            byte tmp;

            if ((scroll.Address & 0x3f00) == 0x3f00)
            {
                tmp = PeekByte(scroll.Address);
                chr = PeekByte((ushort)(scroll.Address & 0x2fff));
            }
            else
            {
                tmp = chr;
                chr = PeekByte(scroll.Address);
            }

            if (Rendering)
            {
                scroll.ClockY();
            }
            else
            {
                scroll.Address = (ushort)((scroll.Address + scroll.Step) & 0x7fff);
            }

            AddressUpdate(scroll.Address);

            data = tmp;
        }

        private void Write2000(ushort address, ref byte data)
        {
            scroll.Temp = (ushort)((scroll.Temp & 0x73ff) | ((data << 10) & 0x0c00));
            scroll.Step = (ushort)((data & 0x04) != 0 ? 0x0020 : 0x0001);
            spr.Address = (ushort)((data & 0x08) != 0 ? 0x1000 : 0x0000);
            bkg.Address = (ushort)((data & 0x10) != 0 ? 0x1000 : 0x0000);
            spr.Rasters = (data & 0x20) != 0 ? 0x0010 : 0x0008;
            vblEnabled = (data & 0x80) >> 7;

            VBL();
        }

        private void Write2001(ushort address, ref byte data)
        {
            bkg.Clipped = (data & 0x02) == 0;
            spr.Clipped = (data & 0x04) == 0;
            bkg.Enabled = (data & 0x08) != 0;
            spr.Enabled = (data & 0x10) != 0;

            clipping = (data & 0x01) != 0 ? 0x30 : 0x3f;
            emphasis = (data & 0xe0) << 1;
        }

        private void Write2003(ushort address, ref byte data)
        {
            oamAddress = data;
            oamAddressLatch = data;
        }

        private void Write2004(ushort address, ref byte data)
        {
            if ((oamAddress & 3) == 2)
                data &= 0xe3;

            oam[oamAddress++] = data;
        }

        private void Write2005(ushort address, ref byte data)
        {
            scroll.Swap = !scroll.Swap;

            if (scroll.Swap)
            {
                scroll.Temp = (ushort)((scroll.Temp & ~0x001f) | ((data & ~7) >> 3));
                scroll.Fine = (data & 0x07);
            }
            else
            {
                scroll.Temp = (ushort)((scroll.Temp & ~0x73e0) | ((data & 7) << 12) | ((data & ~7) << 2));
            }
        }

        private void Write2006(ushort address, ref byte data)
        {
            scroll.Swap = !scroll.Swap;
            if (scroll.Swap)
            {
                scroll.Temp = (ushort)((scroll.Temp & ~0xff00) | ((data & 0x3f) << 8));
            }
            else
            {
                scroll.Temp = (ushort)((scroll.Temp & ~0x00ff) | ((data & 0xff) << 0));
                scroll.Address = (scroll.Temp);

                AddressUpdate(scroll.Address);
            }
        }

        private void Write2007(ushort address, ref byte data)
        {
            PokeByte(scroll.Address, data);

            if (Rendering)
            {
                scroll.ClockY();
            }
            else
            {
                scroll.Address = (ushort)((scroll.Address + scroll.Step) & 0x7fff);
            }

            AddressUpdate(scroll.Address);
        }

        private void ReadPal0(ushort address, ref byte data)
        {
            data = pal[address & 0x000c];
        }

        private void ReadPalN(ushort address, ref byte data)
        {
            data = pal[address & 0x001f];
        }

        private void WritePal0(ushort address, ref byte data)
        {
            pal[address & 0x000c] = (byte)(data & 0x3f);
        }

        private void WritePalN(ushort address, ref byte data)
        {
            pal[address & 0x001f] = (byte)(data & 0x3f);
        }

        private void AddressUpdate(ushort address)
        {
            addressProducer.Produce(new PpuAddressSignal(address));
        }

        private void VBL()
        {
            var signal = new VblNmiSignal(vblFlag & vblEnabled);

            vblNmiProducer.Produce(signal);
        }

        private void ActiveCycle()
        {
            #region V-Active

            if (hclock < 256)
            {
                switch (hclock & 7)
                {
                case 0:
                    PointBgName();
                    RenderPixel();
                    SpriteEvaluation0();
                    break;

                case 1:
                    FetchBgName();
                    RenderPixel();
                    SpriteEvaluation1();
                    break;

                case 2:
                    PointBgAttr();
                    RenderPixel();
                    SpriteEvaluation0();
                    break;

                case 3:
                    FetchBgAttr();
                    RenderPixel();
                    SpriteEvaluation1();
                    break;

                case 4:
                    PointBgBit0();
                    RenderPixel();
                    SpriteEvaluation0();
                    break;

                case 5:
                    FetchBgBit0();
                    RenderPixel();
                    SpriteEvaluation1();
                    break;

                case 6:
                    PointBgBit1();
                    RenderPixel();
                    SpriteEvaluation0();
                    break;

                case 7:
                    FetchBgBit1();
                    RenderPixel();
                    SpriteEvaluation1();
                    SynthesizeBg();

                    if (hclock != 0xff)
                    {
                        scroll.ClockX();
                    }
                    else
                    {
                        scroll.ClockY();
                    }
                    break;
                }

                if (hclock == 0x3f) EvaluationBegin();
                if (hclock == 0xff) EvaluationReset();
            }
            else if (hclock < 320)
            {
                if (hclock == 0x101)
                    scroll.ResetX();

                switch (hclock & 7)
                {
                case 0:
                    PointBgName();
                    break;

                case 1:
                    FetchBgName();
                    break;

                case 2:
                    PointBgAttr();
                    break;

                case 3:
                    FetchBgAttr();
                    break;

                case 4:
                    PointSpBit0();
                    break;

                case 5:
                    FetchSpBit0();
                    break;

                case 6:
                    PointSpBit1();
                    break;

                case 7:
                    FetchSpBit1();
                    SynthesizeSp();
                    break;
                }
            }
            else if (hclock < 336)
            {
                switch (hclock & 7)
                {
                case 0:
                    PointBgName();
                    break;

                case 1:
                    FetchBgName();
                    break;

                case 2:
                    PointBgAttr();
                    break;

                case 3:
                    FetchBgAttr();
                    break;

                case 4:
                    PointBgBit0();
                    break;

                case 5:
                    FetchBgBit0();
                    break;

                case 6:
                    PointBgBit1();
                    break;

                case 7:
                    FetchBgBit1();
                    SynthesizeBg();
                    scroll.ClockX();
                    break;
                }
            }
            else if (hclock < 340)
            {
                switch (hclock & 1)
                {
                case 0:
                    PointBgName();
                    break;

                case 1:
                    FetchBgName();
                    break;
                }
            }

            #endregion
        }

        private void BufferCycle()
        {
            #region V-Buffer

            if (hclock < 256)
            {
                switch (hclock & 7)
                {
                case 0: AddressUpdate(0x2000); break;
                case 2: AddressUpdate(0x23c0); break;
                case 4: AddressUpdate(bkg.Address); break;
                case 6: AddressUpdate(bkg.Address); break;

                case 7:
                    if (hclock != 0xff)
                    {
                        scroll.ClockX();
                    }
                    else
                    {
                        scroll.ClockY();
                    }
                    break;
                }

                bkg.Pixels[hclock] = 0x0000;
                spr.Pixels[hclock] = 0x0000;
            }
            else if (hclock < 320)
            {
                if (hclock == 0x101)
                    scroll.ResetX();

                switch (hclock & 7)
                {
                case 0: AddressUpdate(0x2000); break;
                case 2: AddressUpdate(0x23c0); break;
                case 4: AddressUpdate(spr.Address); break;
                case 6: AddressUpdate(spr.Address); break;
                }
            }
            else if (hclock < 336)
            {
                switch (hclock & 7)
                {
                case 0: PointBgName(); break;
                case 1: FetchBgName(); break;
                case 2: PointBgAttr(); break;
                case 3: FetchBgAttr(); scroll.ClockX(); break;
                case 4: PointBgBit0(); break;
                case 5: FetchBgBit0(); break;
                case 6: PointBgBit1(); break;
                case 7: FetchBgBit1(); SynthesizeBg(); break;
                }
            }
            else if (hclock < 340)
            {
                switch (hclock)
                {
                case 336: AddressUpdate(0x2000); break;
                case 338: AddressUpdate(0x2000); break;
                }
            }

            #endregion

            if (hclock == 304)
            {
                scroll.Address = scroll.Temp;
            }

            if (hclock == 337 && field)
            {
                Tick();
            }
        }

        private void ForcedBlankCycle()
        {
            if (vclock >= 240) return;
            if (hclock >= 256) return;

            var color = (scroll.Address & 0x3f00) == 0x3f00
                ? PeekByte(scroll.Address)
                : PeekByte(0x3f00);

            raster[hclock] = Palette.Ntsc[(color & clipping) | emphasis];
        }

        private void RenderPixel()
        {
            var bkgPixel = bkg.Pixels[hclock + scroll.Fine];
            var sprPixel = spr.Pixels[hclock];
            int pixel;

            if (!bkg.Enabled || (bkg.Clipped && hclock < 8))
            {
                bkgPixel = 0x3f00;
            }

            if (!spr.Enabled || (spr.Clipped && hclock < 8) || hclock == 255)
            {
                sprPixel = 0x3f00;
            }

            if ((bkgPixel & 0x03) == 0)
            {
                pixel = sprPixel;
            }
            else if ((sprPixel & 0x03) == 0)
            {
                pixel = bkgPixel;
            }
            else
            {
                pixel = (sprPixel & 0x8000) != 0
                    ? bkgPixel
                    : sprPixel;

                if ((sprPixel & 0x4000) != 0)
                {
                    sprZerohit = true;
                }
            }

            raster[hclock] = Palette.Ntsc[(PeekByte((ushort)(pixel | 0x3f00)) & clipping) | emphasis];
        }

        private void Tick()
        {
            if (vclock == 240 && hclock == 340)
            {
                vblHold = 1;
            }
            if (vclock == 241 && hclock == 0)
            {
                vblFlag = vblHold;
            }
            if (vclock == 241 && hclock == 2)
            {
                VBL();
            }

            if (vclock == 260 && hclock == 340)
            {
                sprOverrun = false;
                sprZerohit = false;
            }
            if (vclock == 260 && hclock == 340)
            {
                vblHold = 0;
            }
            if (vclock == 261 && hclock == 0)
            {
                vblFlag = vblHold;
            }
            if (vclock == 261 && hclock == 2)
            {
                VBL();
            }

            hclock++;
        }

        private void InitializeMemory()
        {
            bus.Map("0011 1111 ---- ----", reader: ReadPalN, writer: WritePalN);
            bus.Map("0011 1111 ---- --00", reader: ReadPal0, writer: WritePal0);
        }

        private byte PeekByte(ushort address)
        {
            byte data = 0;

            address &= 0x3fff;
            bus.Read(address, ref data);

            return data;
        }

        private void PokeByte(ushort address, byte data)
        {
            address &= 0x3fff;
            bus.Write(address, ref data);
        }

        public void Initialize()
        {
            vclock = 261;
            raster = video.GetRaster(0);

            byte zero = 0;

            Write2000(0x2000, ref zero);
            Write2001(0x2001, ref zero);
            //  $2002: Unimplemented/Invalid
            Write2003(0x2003, ref zero);
            //  $2004: ORAM Data Port (Writing will modify public registers in an undesired manner)
            Write2005(0x2005, ref zero);
            Write2006(0x2006, ref zero);
            //  $2007: VRAM Data Port (Writing will modify public registers in an undesired manner)

            InitializeMemory();
            InitializeSprite();
        }

        public override void Update()
        {
            if (bkg.Enabled || spr.Enabled)
            {
                if (vclock < 240)
                {
                    ActiveCycle();
                }
                if (vclock == 261)
                {
                    BufferCycle();
                }
            }
            else
            {
                ForcedBlankCycle();
            }

            Tick();

            if (hclock == 341)
            {
                hclock = 0;
                vclock++;

                if (vclock == 261)
                {
                    field = !field;
                }

                if (vclock == 262)
                {
                    vclock = 0;

                    frameProducer.Produce(new FrameSignal());

                    video.Render();
                }

                if (vclock < 240)
                {
                    raster = video.GetRaster(vclock);
                }
            }
        }

        public void MapTo(IBus bus)
        {
            bus.Map("001- ---- ---- -000", writer: Write2000);
            bus.Map("001- ---- ---- -001", writer: Write2001);
            bus.Map("001- ---- ---- -010", reader: Read2002);
            bus.Map("001- ---- ---- -011", writer: Write2003);
            bus.Map("001- ---- ---- -100", reader: Read2004, writer: Write2004);
            bus.Map("001- ---- ---- -101", writer: Write2005);
            bus.Map("001- ---- ---- -110", writer: Write2006);
            bus.Map("001- ---- ---- -111", reader: Read2007, writer: Write2007);
        }

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles);
        }

        private class Fetch
        {
            public byte Attr;
            public byte Bit0;
            public byte Bit1;
            public byte Name;
            public ushort Address;
        }

        private class Scroll
        {
            public bool Swap;
            public int Fine;
            public ushort Step = 1;
            public ushort Address;
            public ushort Temp;

            public void ClockX()
            {
                if ((Address & 0x001f) == 0x001f)
                    Address ^= 0x041f;
                else
                    Address += 0x0001;
            }

            public void ClockY()
            {
                if ((Address & 0x7000) != 0x7000)
                {
                    Address += 0x1000;
                }
                else
                {
                    switch (Address & 0x3e0)
                    {
                    case 0x3a0:
                        Address ^= 0x7ba0;
                        break;

                    case 0x3e0:
                        Address ^= 0x73e0;
                        break;

                    default:
                        Address += 0x1020;
                        break;
                    }
                }
            }

            public void ResetX()
            {
                Address = (ushort)((Address & ~0x041f) | (Temp & 0x041f));
            }

            public void ResetY()
            {
                Address = (ushort)((Address & ~0x7be0) | (Temp & 0x7be0));
            }
        }

        private class Synthesizer
        {
            public bool Clipped;
            public bool Enabled;
            public ushort Address;
            public int Rasters = 8;
            public int[] Pixels;

            public Synthesizer(int capacity)
            {
                Pixels = new int[capacity];
            }
        }
    }
}
