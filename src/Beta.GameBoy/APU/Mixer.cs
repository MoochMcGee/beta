using Beta.Platform.Audio;

namespace Beta.GameBoy.APU
{
    public static class Mixer
    {
        private static readonly int[][] square_lut = new[]
        {
            new[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 1, 1, 1 },
            new[] { 0, 1, 1, 1, 1, 1, 1, 0 }
        };

        public static void RenderSample(ApuState e, IAudioBackend audio)
        {
            var sq1 = RenderSq1Sample(e.sq1);
            var sq2 = RenderSq2Sample(e.sq2);
            var wav = RenderWavSample(e.wav);
            var noi = RenderNoiSample(e.noi);

            for (int speaker = 0; speaker < 2; speaker++)
            {
                var sample = 0;
                var select = e.speaker_select[speaker];
                var volume = e.speaker_volume[speaker];

                if ((select & 1) != 0) sample += sq1;
                if ((select & 2) != 0) sample += sq2;
                if ((select & 4) != 0) sample += wav;
                if ((select & 8) != 0) sample += noi;

                // apply volume correction

                sample = (sample * volume * 32768) / 16 / 4 / 8;

                audio.Render(sample);
            }
        }

        static int RenderSq1Sample(Sq1State e)
        {
            return e.enabled
                ? e.envelope.counter * square_lut[e.duty_form][e.duty_step]
                : 0
                ;
        }

        static int RenderSq2Sample(Sq2State e)
        {
            return e.enabled
                ? e.envelope.counter * square_lut[e.duty_form][e.duty_step]
                : 0
                ;
        }

        static int RenderWavSample(WavState e)
        {
            return e.enabled
                ? e.wave_ram_output >> e.volume_shift
                : 0
                ;
        }

        static int RenderNoiSample(NoiState e)
        {
            return e.enabled
                ? e.envelope.counter * (~e.lfsr & 1)
                : 0
                ;
        }
    }
}
