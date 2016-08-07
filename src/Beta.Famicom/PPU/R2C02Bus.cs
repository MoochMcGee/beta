namespace Beta.Famicom.PPU
{
    public sealed class R2C02Bus : Bus
    {
        public R2C02Bus(R2C02MemoryMap memory)
            : base(1 << 14)
        {
            Map("001- ---- ---- ----", memory.Read, memory.Write);
        }
    }
}
