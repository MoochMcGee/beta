using Beta.GameBoy.APU;

namespace Beta.GameBoy.Memory
{
    public sealed class Registers
    {
        public ApuRegisters apu = new ApuRegisters();
        public CpuRegisters cpu = new CpuRegisters();
        public PadRegisters pad = new PadRegisters();
        public PpuRegisters ppu = new PpuRegisters();
        public TmaRegisters tma = new TmaRegisters();

        public NoiRegisters noi = new NoiRegisters();
        public Sq1Registers sq1 = new Sq1Registers();
        public Sq2Registers sq2 = new Sq2Registers();
        public WavRegisters wav = new WavRegisters();

        public bool boot_rom_enabled = true;
    }

    public sealed class CpuRegisters
    {
        public byte ief;
        public byte irf;
    }

    public sealed class PadRegisters
    {
        public bool p14;
        public bool p15;
        public byte p14_latch;
        public byte p15_latch;
    }

    public sealed class PpuRegisters
    {
        public bool bkg_enabled;
        public bool lcd_enabled;
        public bool obj_enabled;
        public bool wnd_enabled;
        public byte bkg_palette;
        public byte[] obj_palette = new byte[2];
        public byte scroll_x;
        public byte scroll_y;
        public byte window_x;
        public byte window_y;
        public int bkg_char_address = 0x1000;
        public int bkg_name_address = 0x1800;
        public int wnd_name_address = 0x1800;
        public int obj_rasters = 8;
        public int control;
        public int h;
        public byte v;
        public byte v_check;
        public byte ff40;

        public bool dma_triggered;
        public byte dma_segment;
    }

    public sealed class TmaRegisters
    {
        public byte divider;
        public byte counter;
        public byte control;
        public byte modulus;

        public int divider_prescaler;
        public int counter_prescaler;
    }
}
