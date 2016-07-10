using Beta.Platform;

namespace Beta.SuperFamicom
{
    public sealed class State
    {
        public readonly SCpuState scpu = new SCpuState();
    }

    public sealed class DmaState
    {
        public ushort count;
        public Register24 address_a;
        public byte address_b;
        public byte control;
    }

    public sealed class SCpuState
    {
        public bool in_hblank;
        public bool in_vblank;
        public bool timer_coincidence;

        public int h;
        public int h_target;
        public int v;
        public int v_target;
        public int reg4200;

        public DmaState[] dma = new DmaState[8];

        public SCpuState()
        {
            dma.Initialize(() => new DmaState());
        }
    }
}
