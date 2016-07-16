using Beta.Platform;

namespace Beta.SuperFamicom
{
    public sealed class State
    {
        public readonly SCpuState scpu = new SCpuState();
        public readonly SPpuState sppu = new SPpuState();
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
        public bool fast_cart;

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
        public byte mdma_en;
        public byte hdma_en;
        public int mdma_count;

        public SCpuState()
        {
            dma.Initialize(() => new DmaState());
        }
    }

    public sealed class SPpuState
    {
        public Mode7State m7 = new Mode7State();

        public BackgroundState bg0 = new BackgroundState(0);
        public BackgroundState bg1 = new BackgroundState(1);
        public BackgroundState bg2 = new BackgroundState(2);
        public BackgroundState bg3 = new BackgroundState(3);

        public WindowState window1 = new WindowState();
        public WindowState window2 = new WindowState();

        public bool bg_priority;
        public int bg_mode;
        public int bg_mosaic_size;
        public int bg_offset_latch;
    }

    public sealed class BackgroundState
    {
        public readonly int index;

        public int h_offset;
        public int v_offset;

        public bool mosaic;

        public int name_base;
        public int name_size;
        public int char_base;
        public int char_size;

        public BackgroundState(int index)
        {
            this.index = index;
        }
    }

    public sealed class Mode7State
    {
        public byte control;
        public byte latch;
        public ushort a;
        public ushort b;
        public ushort c;
        public ushort d;
        public ushort x;
        public ushort y;
        public ushort h_offset;
        public ushort v_offset;
    }

    public sealed class WindowState
    {
        public byte x1;
        public byte x2;
    }
}
