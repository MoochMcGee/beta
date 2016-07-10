namespace Beta.SuperFamicom
{
    public sealed class State
    {
        public readonly SCpuState scpu = new SCpuState();
    }

    public sealed class SCpuState
    {
        public bool in_hblank;
        public bool in_vblank;

        public int reg4200;
    }
}
