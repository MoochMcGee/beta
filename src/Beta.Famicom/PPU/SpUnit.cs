using Beta.Platform;

namespace Beta.Famicom.PPU
{
    public static class SpUnit
    {
        public static void Evaluation0(R2C02State e)
        {
            e.spLatch = (byte)(e.h < 64 ? 0xff : e.oam[e.oam_address]);
        }

        public static void Evaluation1(R2C02State e)
        {
            if (e.h < 64)
            {
                switch ((e.h >> 1) & 3)
                {
                case 0: e.spFound[(e.h >> 3) & 7].Y    = e.spLatch; break;
                case 1: e.spFound[(e.h >> 3) & 7].Name = e.spLatch; break;
                case 2: e.spFound[(e.h >> 3) & 7].Attr = e.spLatch &= 0xe3; break;
                case 3: e.spFound[(e.h >> 3) & 7].X    = e.spLatch; break;
                }
            }
            else
            {
                switch (e.spPhase)
                {
                case 0:
                    {
                        e.spCount++;

                        var raster = (e.v - e.spLatch) & 0x1ff;
                        if (raster < e.obj_rasters)
                        {
                            e.oam_address++;
                            e.spFound[e.spIndex].Y = e.spLatch;
                            e.spPhase++;
                        }
                        else
                        {
                            if (e.spCount != 64)
                            {
                                e.oam_address += 4;
                            }
                            else
                            {
                                e.oam_address = 0;
                                e.spPhase = 8;
                            }
                        }
                    }
                    break;

                case 1:
                    e.oam_address++;
                    e.spFound[e.spIndex].Name = e.spLatch;
                    e.spPhase++;
                    break;

                case 2:
                    e.oam_address++;
                    e.spFound[e.spIndex].Attr = e.spLatch &= 0xe3;
                    e.spPhase++;

                    if (e.spCount == 1)
                    {
                        e.spFound[e.spIndex].Attr |= R2C02State.Sprite.SPR_ZERO;
                    }
                    break;

                case 3:
                    e.spFound[e.spIndex].X = e.spLatch;
                    e.spIndex++;

                    if (e.spCount != 64)
                    {
                        e.spPhase = (e.spIndex != 8 ? 0 : 4);
                        e.oam_address++;
                    }
                    else
                    {
                        e.spPhase = 8;
                        e.oam_address = 0;
                    }
                    break;

                case 4:
                    {
                        var raster = (e.v - e.spLatch) & 0x1ff;
                        if (raster < e.obj_rasters)
                        {
                            e.obj_overflow = true;
                            e.spPhase++;
                            e.oam_address++;
                        }
                        else
                        {
                            e.oam_address = (byte)(((e.oam_address + 4) & ~3) + ((e.oam_address + 1) & 3));

                            if (e.oam_address <= 5)
                            {
                                e.spPhase = 8;
                                e.oam_address &= 0xfc;
                            }
                        }
                    }
                    break;

                case 5:
                    e.spPhase = 6;
                    e.oam_address++;
                    break;

                case 6:
                    e.spPhase = 7;
                    e.oam_address++;
                    break;

                case 7:
                    e.spPhase = 8;
                    e.oam_address++;
                    break;

                case 8:
                    e.oam_address += 4;
                    break;
                }
            }
        }

        public static void EvaluationBegin(R2C02State e)
        {
            e.oam_address = 0;

            e.spCount = 0;
            e.spIndex = 0;
            e.spPhase = 0;
        }

        public static void EvaluationReset(R2C02State e)
        {
            EvaluationBegin(e);

            for (var i = 0; i < 0x100; i++)
            {
                e.spPixel[i] = 0;
            }
        }

        public static void PointBit0(R2C02State e)
        {
            var sprite = e.spFound[(e.h >> 3) & 7];
            var raster = e.v - sprite.Y;

            if ((sprite.Attr & R2C02State.Sprite.V_FLIP) != 0)
                raster ^= 0xf;

            if (e.obj_rasters == 8)
            {
                e.fetch_address = (sprite.Name << 4) | (raster & 7) | e.obj_address;
            }
            else
            {
                sprite.Name = (byte)((sprite.Name >> 1) | (sprite.Name << 7));

                e.fetch_address = (sprite.Name << 5) | (raster & 7) | (raster << 1 & 0x10);
            }

            e.fetch_address |= 0;
        }

        public static void PointBit1(R2C02State e)
        {
            e.fetch_address |= 8;
        }

        public static void FetchBit0(R2C02State e, R2C02MemoryMap memory)
        {
            var sprite = e.spFound[(e.h >> 3) & 7];

            memory.Read(e.fetch_address, ref e.fetch_bit0);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                e.fetch_bit0 = 0;
            }
            else if ((sprite.Attr & R2C02State.Sprite.H_FLIP) != 0)
            {
                e.fetch_bit0 = Utility.ReverseLookup[e.fetch_bit0];
            }
        }

        public static void FetchBit1(R2C02State e, R2C02MemoryMap memory)
        {
            var sprite = e.spFound[(e.h >> 3) & 7];

            memory.Read(e.fetch_address, ref e.fetch_bit1);

            if (sprite.X == 255 || sprite.Y == 255)
            {
                e.fetch_bit1 = 0;
            }
            else if ((sprite.Attr & R2C02State.Sprite.H_FLIP) != 0)
            {
                e.fetch_bit1 = Utility.ReverseLookup[e.fetch_bit1];
            }
        }

        public static void InitializeSprite(R2C02State e)
        {
            for (var i = 0; i < 8; i++)
            {
                e.spFound[i] = new R2C02State.Sprite();
            }

            e.spLatch = 0;
            e.spCount = 0;
            e.spIndex = 0;
            e.spPhase = 0;
        }

        public static void Synthesize(R2C02State e)
        {
            if (e.v == 261)
            {
                return;
            }

            var sprite = e.spFound[(e.h >> 3) & 7];
            int offset = sprite.X;

            for (var i = 0; i < 8 && offset < 256; i++, offset++)
            {
                var color =
                    ((e.fetch_bit0 >> 7) & 1) |
                    ((e.fetch_bit1 >> 6) & 2);

                if ((e.spPixel[offset] & 3) == 0 && color != 0)
                {
                    e.spPixel[offset] = 0x10 | ((sprite.Attr << 10) & 0xc000) | ((sprite.Attr << 2) & 12) | color;
                }

                e.fetch_bit0 <<= 1;
                e.fetch_bit1 <<= 1;
            }
        }

        public static int GetPixel(R2C02State e)
        {
            if (e.obj_enabled == false)
            {
                return 0;
            }

            if (e.obj_clipped && e.h < 8)
            {
                return 0;
            }

            if (e.h == 255)
            {
                return 0;
            }

            return e.spPixel[e.h];
        }
    }
}
