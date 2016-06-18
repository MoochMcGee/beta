using Beta.Famicom.Database;
using Beta.Famicom.Memory;

namespace Beta.Famicom.Formats
{
    public sealed class CartridgeImage
    {
        public IMemory[] PRG { get; set; }
        public IMemory[] CHR { get; set; }
        public IMemory[] WRAM { get; set; }
        public IMemory[] VRAM { get; set; }
        public Chip[] Chips { get; set; }
        public string Mapper { get; set; }
        public int H { get; set; }
        public int V { get; set; }
    }
}
