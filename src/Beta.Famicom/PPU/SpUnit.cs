using Beta.Platform;

namespace Beta.Famicom.PPU
{
    public sealed class SpUnit
    {
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State state;
        private readonly int[] pixel;

        private Sprite[] spFound = new Sprite[8];
        private byte spLatch;
        private int spCount;
        private int spIndex;
        private int spPhase;

        public SpUnit(R2C02MemoryMap memory, State state)
        {
            this.memory = memory;
            this.state = state.r2c02;
            this.pixel = new int[256];
        }

        public void Synthesize()
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

                if ((pixel[offset] & 3) == 0 && color != 0)
                {
                    pixel[offset] = 0x10 | ((sprite.Attr << 10) & 0xc000) | ((sprite.Attr << 2) & 12) | color;
                }

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        public void Evaluation0()
        {
            spLatch = (byte)(state.h < 64 ? 0xff : state.oam[state.oam_address]);
        }

        public void Evaluation1()
        {
            if (state.h < 64)
            {
                switch ((state.h >> 1) & 3)
                {
                case 0: spFound[(state.h >> 3) & 7].Y = spLatch; break;
                case 1: spFound[(state.h >> 3) & 7].Name = spLatch; break;
                case 2: spFound[(state.h >> 3) & 7].Attr = spLatch &= 0xe3; break;
                case 3: spFound[(state.h >> 3) & 7].X = spLatch; break;
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

        public void EvaluationBegin()
        {
            state.oam_address = 0;

            spCount = 0;
            spIndex = 0;
            spPhase = 0;
        }

        public void EvaluationReset()
        {
            EvaluationBegin();

            for (var i = 0; i < 0x100; i++)
            {
                pixel[i] = 0;
            }
        }

        public void PointBit0()
        {
            var sprite = spFound[(state.h >> 3) & 7];
            var raster = state.v - sprite.Y;

            if ((sprite.Attr & Sprite.V_FLIP) != 0)
                raster ^= 0xf;

            if (state.obj_rasters == 8)
            {
                state.fetch_address = (sprite.Name << 4) | (raster & 7) | state.obj_address;
            }
            else
            {
                sprite.Name = (byte)((sprite.Name >> 1) | (sprite.Name << 7));

                state.fetch_address = (sprite.Name << 5) | (raster & 7) | (raster << 1 & 0x10);
            }

            state.fetch_address |= 0;
        }

        public void PointBit1()
        {
            state.fetch_address |= 8;
        }

        public void FetchBit0()
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

        public void FetchBit1()
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

        public void InitializeSprite()
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

        public void ResetSprite()
        {
            spLatch = 0;
            spCount = 0;
            spIndex = 0;
            spPhase = 0;

            state.oam_address = 0;
        }

        public int GetPixel()
        {
            if (state.obj_enabled == false)
            {
                return 0;
            }

            if (state.obj_clipped && state.h < 8)
            {
                return 0;
            }

            if (state.h == 255)
            {
                return 0;
            }

            return pixel[state.h];
        }

        public class Sprite
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
    }
}
