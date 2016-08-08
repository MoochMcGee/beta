using Beta.Famicom.Memory;

namespace Beta.Famicom.Formats
{
    public sealed class CartridgeImage
    {
        public IMemory prg;
        public IMemory chr;
        public IMemory wram;
        public IMemory vram;
        public int h;
        public int v;
        public string mapper;
    }
}
