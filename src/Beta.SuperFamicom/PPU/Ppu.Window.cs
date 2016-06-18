namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private Window window1 = new Window();
        private Window window2 = new Window();

        private sealed class Window
        {
            public bool Dirty;
            public byte L;
            public byte R;

            public int[] MaskBuffer = new int[256];

            public void Update()
            {
                if (L > R)
                {
                    for (var i = 0; i < 256; i++) MaskBuffer[i] = 0;
                }
                else
                {
                    for (var i = 0; i < L; i++) MaskBuffer[i] = 0;
                    for (int i = L; i < R; i++) MaskBuffer[i] = 1;
                    for (int i = R; i < 256; i++) MaskBuffer[i] = 0;
                }
            }
        }
    }
}
