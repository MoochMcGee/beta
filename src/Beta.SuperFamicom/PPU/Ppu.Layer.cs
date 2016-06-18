using Beta.Platform;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private abstract class Layer
        {
            private int w1Reg;
            private int w2Reg;
            private int w1Inv;
            private int w2Inv;
            private int wnLog;
            private int[] window = new int[256];

            protected Ppu Ppu;

            public bool WnDirty;
            public bool Wm;
            public bool Ws;
            public int Sm;
            public int Ss;
            public int W1;
            public int W2;
            public bool[] Enable = new bool[256];
            public int[] Raster = new int[256];
            public int[] Priority = new int[256];
            public int[] Priorities;

            protected Layer(Ppu ppu, int priorities)
            {
                Ppu = ppu;
                Priorities = new int[priorities];
                window.Initialize(-1);
            }

            public virtual int GetColorM(int index)
            {
                var color = Raster[index] & Sm;

                if (Wm) color &= window[index];

                return color;
            }

            public virtual int GetColorS(int index)
            {
                var color = Raster[index] & Ss;

                if (Ws) color &= window[index];

                return color;
            }

            public virtual void Initialize()
            {
            }

            public void PokeWindow1(byte value)
            {
                if (w1Reg == (value & 15u))
                {
                    return;
                }

                w1Reg =  ((value & 0xf) >> 0);
                w1Inv =  ((value & 0x1) >> 0);
                W1    = ~((value & 0x2) >> 1) + 1;
                w2Inv =  ((value & 0x4) >> 2);
                W2    = ~((value & 0x8) >> 3) + 1;
                WnDirty = true;
            }

            public void PokeWindow2(byte value)
            {
                if (w2Reg == (value & 3u))
                {
                    return;
                }

                w2Reg = ((value & 0x3) >> 0);
                wnLog = ((value & 0x3) >> 0);
                WnDirty = true;
            }

            public void UpdateWindow()
            {
                var w1Buffer = Ppu.window1.MaskBuffer;
                var w2Buffer = Ppu.window2.MaskBuffer;

                for (var i = 0; i < 256; i++)
                {
                    var w1Mask = (w1Buffer[i] ^ w1Inv) & W1;
                    var w2Mask = (w2Buffer[i] ^ w2Inv) & W2;

                    switch (wnLog & W1 & W2)
                    {
                    case 0: window[i] = ((w1Mask | w2Mask) ^ 0) - 1; break; // or
                    case 1: window[i] = ((w1Mask & w2Mask) ^ 0) - 1; break; // and
                    case 2: window[i] = ((w1Mask ^ w2Mask) ^ 0) - 1; break; // xor
                    case 3: window[i] = ((w1Mask ^ w2Mask) ^ 1) - 1; break; // xnor
                    }
                }
            }
        }
    }
}
