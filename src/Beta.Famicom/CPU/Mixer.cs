using Beta.Platform.Audio;

namespace Beta.Famicom.CPU
{
    public sealed class Mixer
    {
        private const int SAMPLE_STEP = 528000; // 11 * 48,000
        private const int SAMPLE_PERIOD = 19687500; // 11 * 1,789,772.72~

        public static void Tick(IAudioBackend audio, R2A03State e)
        {
            e.sample_prescaler += SAMPLE_STEP;

            if (e.sample_prescaler >= SAMPLE_PERIOD)
            {
                e.sample_prescaler -= SAMPLE_PERIOD;
                Sample(audio, e);
            }
        }

        private static void Sample(IAudioBackend audio, R2A03State e)
        {
            var sq1 = SQ1.GetOutput(e.sq1);
            var sq2 = SQ2.GetOutput(e.sq2);
            var tri = TRI.GetOutput(e.tri);
            var noi = NOI.GetOutput(e.noi);
            var dmc = DMC.GetOutput(e.dmc);

            var output = MixSamples(sq1, sq2, tri, noi, dmc);

            audio.Render(output);
        }

        private static int MixSamples(int sq1, int sq2, int tri, int noi, int dmc)
        {
            const double sqr_base = 95.52;
            const double sqr_div = 8128.0;

            const double tnd_base = 163.67;
            const double tnd_div = 24329.0;

            var sqr_n = sq1 + sq2;
            var sqr = sqr_base / (sqr_div / sqr_n + 100);

            var tnd_n = tri * 3 + noi * 2 + dmc;
            var tnd = tnd_base / (tnd_div / tnd_n + 100);

            return (int)((sqr + tnd) * 32767);
        }
    }
}
