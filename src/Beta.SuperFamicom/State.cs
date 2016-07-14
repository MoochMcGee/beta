using Beta.Platform;

namespace Beta.SuperFamicom
{
    public sealed class State
    {
        public readonly SCpuState scpu = new SCpuState();
        public readonly ushort[] pads = new ushort[2];
    }

    public sealed class DmaState
    {
        public ushort count;
        public int address_a;
        public byte address_b;
        public byte control;
    }

    public sealed class SCpuState
    {
        public bool in_hblank;
        public bool in_vblank;
        public bool timer_coincidence;

        public int dram_prescaler = 8;
        public int dram_timer = 538;
        public int time_prescaler = 4;

        public int h;
        public int h_target;
        public int v;
        public int v_target;
        public int reg4200;

        // multiply regs
        public byte wrmpya;
        public byte wrmpyb;
        public ushort rdmpy;

        // divide regs
        public ushort wrdiv;
        public byte wrdivb;
        public ushort rddiv;

        public DmaState[] dma = new DmaState[8];

        public SCpuState()
        {
            dma.Initialize(() => new DmaState());
        }
    }
}
