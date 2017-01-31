namespace Beta.GameBoy.APU
{
    public sealed class ApuState
    {
        public int sample_timer;
        public int sample_period = 4194304;

        public int sequence_timer = 4194304 / 512;
        public int sequence_step;

        public bool enabled;
        public int output_vin_l;
        public int output_vin_r;
        public int[] speaker_select = new int[2];
        public int[] speaker_volume = new int[2];

        public Sq1State sq1 = new Sq1State();
        public Sq2State sq2 = new Sq2State();
        public WavState wav = new WavState();
        public NoiState noi = new NoiState();
    }
}
