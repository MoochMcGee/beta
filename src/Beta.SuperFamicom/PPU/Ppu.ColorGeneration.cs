namespace Beta.SuperFamicom.PPU
{
    public partial class Ppu
    {
        private ColorGeneration clr;

        private sealed class ColorGeneration : Layer
        {
            public ColorGeneration(Ppu ppu)
                : base(ppu, 0)
            {
            }
        }
    }
}
