using Beta.Platform.Audio;
using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class Mixer
    {
        private const int SAMPLE_STEP = 528000; // 11 * 48,000
        private const int SAMPLE_PERIOD = 19687500; // 11 * 1,789,772.72~

        private static readonly int[][] square_lut = new[]
        {
            new[] { 0, 1, 0, 0, 0, 0, 0, 0 },
            new[] { 0, 1, 1, 0, 0, 0, 0, 0 },
            new[] { 0, 1, 1, 1, 1, 0, 0, 0 },
            new[] { 1, 0, 0, 1, 1, 1, 1, 1 }
        };

        private static readonly int[] triangle_lut = new[]
        {
            0xf, 0xe, 0xd, 0xc, 0xb, 0xa, 0x9, 0x8, 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0,
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf
        };

        private readonly R2A03State state;
        private readonly Sq1State sq1;
        private readonly Sq2State sq2;
        private readonly TriState tri;
        private readonly NoiState noi;
        private readonly DmcState dmc;
        private readonly IAudioBackend audio;

        public Mixer(State state, IAudioBackend audio)
        {
            this.state = state.r2a03;
            this.sq1 = state.r2a03.sq1;
            this.sq2 = state.r2a03.sq2;
            this.tri = state.r2a03.tri;
            this.noi = state.r2a03.noi;
            this.dmc = state.r2a03.dmc;
            this.audio = audio;
        }

        public void Consume(ClockSignal e)
        {
            state.sample_prescaler += SAMPLE_STEP;

            if (state.sample_prescaler >= SAMPLE_PERIOD)
            {
                state.sample_prescaler -= SAMPLE_PERIOD;
                Sample();
            }
        }

        private void Sample()
        {
            int sq1 = GetSq1Output();
            int sq2 = GetSq2Output();
            int tri = GetTriOutput();
            int noi = GetNoiOutput();
            int dmc = GetDmcOutput();

            int output = MixSamples(sq1, sq2, tri, noi, dmc);

            audio.Render(output);
        }

        private int GetSq1Output()
        {
            if (sq1.period < 8 || (sq1.sweep.target & 0x800) != 0)
            {
                return 0;
            }

            return sq1.duration.counter != 0 && square_lut[sq1.duty_form][sq1.duty_step] == 1
                ? Envelope.Volume(sq1.envelope)
                : 0
                ;
        }

        private int GetSq2Output()
        {
            if (sq2.period < 8 || (sq2.sweep.target & 0x800) != 0)
            {
                return 0;
            }

            return sq2.duration.counter != 0 && square_lut[sq2.duty_form][sq2.duty_step] == 1
                ? Envelope.Volume(sq2.envelope)
                : 0
                ;
        }

        private int GetTriOutput()
        {
            if (tri.period == 0 || tri.period == 1)
            {
                return 7;
            }
            else
            {
                return triangle_lut[tri.step];
            }
        }

        private int GetNoiOutput()
        {
            return noi.duration.counter != 0 && (noi.lfsr & 1) == 0
                ? Envelope.Volume(noi.envelope)
                : 0
                ;
        }

        private int GetDmcOutput()
        {
            return 0;
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
