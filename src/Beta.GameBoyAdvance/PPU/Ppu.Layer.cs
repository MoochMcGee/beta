namespace Beta.GameBoyAdvance.PPU
{
    public partial class Ppu
    {
        private class Layer
        {
            public bool[] Enable = new bool[240];
            public int[] Priority = new int[240];
            public int[] Raster = new int[240];

            public bool MasterEnable;
            public int Index;

            public virtual void Initialize()
            {
            }
        }
    }
}
