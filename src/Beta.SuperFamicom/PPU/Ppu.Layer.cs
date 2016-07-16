using Beta.Platform.Exceptions;

namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private abstract class Layer
        {
            private WindowState window1;
            private WindowState window2;

            public bool window_main;
            public bool window_sub;
            public int screen_main;
            public int screen_sub;
            public int window_logic;
            public bool window_1_enable;
            public bool window_1_inverted;
            public bool window_2_enable;
            public bool window_2_inverted;
            public bool[] enable = new bool[256];
            public int[] raster = new int[256];
            public int[] priority = new int[256];
            public int[] priorities;

            protected Layer(Ppu ppu, int priorities)
            {
                this.window1 = ppu.sppu.window1;
                this.window2 = ppu.sppu.window2;
                this.priorities = new int[priorities];
            }

            public int GetMainColor(int x)
            {
                return (window_main && GetWindow(x))
                    ? 0
                    : raster[x] & screen_main
                    ;
            }

            public int GetSubColor(int x)
            {
                return (window_sub && GetWindow(x))
                    ? 0
                    : raster[x] & screen_sub
                    ;
            }

            public bool GetWindow(int x)
            {
                if (window_1_enable == false &&
                    window_2_enable == false)
                {
                    return false;
                }

                var w1 = (x >= window1.x1 && x <= window1.x2);
                var w2 = (x >= window2.x1 && x <= window2.x2);

                w1 = (w1 ^ window_1_inverted) && window_1_enable;
                w2 = (w2 ^ window_2_inverted) && window_2_enable;

                switch (window_logic)
                {
                case 0: return  (w1 | w2); // or
                case 1: return  (w1 & w2); // and
                case 2: return  (w1 ^ w2); // xor
                case 3: return !(w1 ^ w2); // xnor
                }

                throw new CompilerPleasingException();
            }
        }
    }
}
