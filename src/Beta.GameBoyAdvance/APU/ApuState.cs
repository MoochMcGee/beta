using Beta.Platform;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class ApuState
    {
        public readonly Sq1State sound_1 = new Sq1State();
        public readonly Sq2State sound_2 = new Sq2State();
        public readonly WavState sound_3 = new WavState();
        public readonly NoiState sound_4 = new NoiState();
        public readonly PcmState sound_a = new PcmState();
        public readonly PcmState sound_b = new PcmState();
    }

    public sealed class Sq1State
    {
        public Duration duration = new Duration(64);
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int frequency;
        public int cycles;
        public int period;

        public bool[] output = new bool[2];

        public int duty_form;
        public int duty_step = 7;

        public bool sweep_enable;
        public int sweep_cycles;
        public int sweep_delta = 1;
        public int sweep_shift;
        public int sweep_shadow;
        public int sweep_period;
    }

    public sealed class Sq2State
    {
        public Duration duration = new Duration(64);
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int frequency;
        public int cycles;
        public int period;

        public bool[] output = new bool[2];

        public int duty_form;
        public int duty_step = 7;
    }

    public sealed class WavState
    {
        public Duration duration = new Duration(256);
        public byte[] registers = new byte[8];

        public bool active;
        public int frequency;
        public int cycles;
        public int period;

        public bool[] output = new bool[2];

        public byte[][] amp = Utility.CreateArray<byte>(2, 32);
        public byte[][] ram = Utility.CreateArray<byte>(2, 16);
        public int bank;
        public int count;
        public int dimension;
        public int shift = 4;
    }

    public sealed class NoiState
    {
        public Duration duration = new Duration(64);
        public Envelope envelope = new Envelope();
        public byte[] registers = new byte[8];

        public bool active;
        public int cycles;
        public int period;

        public bool[] output = new bool[2];

        public int shift = 8;
        public int value = 0x6000;
    }

    public sealed class PcmState { }
}
