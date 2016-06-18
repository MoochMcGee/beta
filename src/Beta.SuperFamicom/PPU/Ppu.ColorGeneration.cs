using System;

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

            public override int GetColorM(int index)
            {
                throw new NotImplementedException();
            }

            public override int GetColorS(int index)
            {
                throw new NotImplementedException();
            }
        }
    }
}
