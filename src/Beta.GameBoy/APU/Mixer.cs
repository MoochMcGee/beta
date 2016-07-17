using Beta.Platform.Audio;

namespace Beta.GameBoy.APU
{
    public sealed class Mixer
    {
        private static int[][] square_lut = new[]
        {
            new[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 1, 1, 1 },
            new[] { 0, 1, 1, 1, 1, 1, 1, 0 }
        };

        private readonly IAudioBackend audio;
        private readonly ApuState apu;
        private readonly Sq1State sq1;
        private readonly Sq2State sq2;
        private readonly WavState wav;
        private readonly NoiState noi;

        public Mixer(IAudioBackend audio, State state)
        {
            this.audio = audio;

            this.apu = state.apu;
            this.sq1 = state.apu.sq1;
            this.sq2 = state.apu.sq2;
            this.wav = state.apu.wav;
            this.noi = state.apu.noi;
        }

        public void RenderSample()
        {
            int sq1_out = sq1.enabled
                ? sq1.envelope.count * square_lut[sq1.duty_form][sq1.duty_step]
                : 0
                ;

            int sq2_out = sq2.enabled
                ? sq2.envelope.count * square_lut[sq2.duty_form][sq2.duty_step]
                : 0
                ;

            int wav_out = wav.enabled
                ? wav.wave_ram_output >> wav.volume_shift
                : 0
                ;

            int noi_out = noi.enabled
                ? noi.envelope.count * (~noi.lfsr & 1)
                : 0
                ;

            for (int speaker = 0; speaker < 2; speaker++)
            {
                var sample = 0;
                var select = apu.speaker_select[speaker];
                var volume = apu.speaker_volume[speaker];

                if ((select & 8) != 0) sample += noi_out;
                if ((select & 4) != 0) sample += wav_out;
                if ((select & 2) != 0) sample += sq2_out;
                if ((select & 1) != 0) sample += sq1_out;

                // apply volume correction

                sample = (sample * volume * 32768) / 16 / 4 / 8;

                audio.Render(sample);
            }
        }
    }
}
