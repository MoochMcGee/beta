using Beta.Famicom.CPU;
using Beta.Famicom.PPU;

namespace Beta.Famicom
{
    public sealed class State
    {
        public readonly R2A03State r2a03 = new R2A03State();
        public readonly R2C02State r2c02 = new R2C02State();
    }
}
