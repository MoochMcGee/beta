namespace Beta.Famicom.CPU
{
    public sealed class R2A03Bus : Bus
    {
        public R2A03Bus(R2A03MemoryMap memory)
            : base(1 << 16)
        {
            Map("000- ---- ---- ----", memory.Read, memory.Write);

            Map("0100 0000 0000 ----", memory.Read, memory.Write);
            Map("0100 0000 0001 0---", memory.Read, memory.Write);
        }
    }
}
