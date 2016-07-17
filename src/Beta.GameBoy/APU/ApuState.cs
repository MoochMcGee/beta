namespace Beta.GameBoy.APU
{
    public sealed class ApuState
    {
        public int sample_timer;
        public int sample_period = 4194304;

        public int sequence_timer = 4194304 / 2048;
        public int sequence_step;

        public bool enabled;
        public int output_vin_l;
        public int output_vin_r;
        public int[] speaker_select = new int[2];
        public int[] speaker_volume = new int[2];

        public readonly Sq1State sq1 = new Sq1State();
        public readonly Sq2State sq2 = new Sq2State();
        public readonly WavState wav = new WavState();
        public readonly NoiState noi = new NoiState();
    }
}
