using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        private ChannelSqr sq1;
        private ChannelSqr sq2;
        private ChannelTri tri;
        private ChannelNoi noi;
        private ChannelDmc dmc;
        private ChannelExt ext;
        private Register16 reg4017;
        private bool irqEnabled;
        private bool irqPending;
        private bool apuToggle;
        private int mode;
        private int step;

        private int sampleTimer;

        private void ClockHalf()
        {
            sq1.Duration.Clock();
            sq2.Duration.Clock();
            tri.Duration.Clock();
            noi.Duration.Clock();

            sq1.ClockSweep(-1);
            sq2.ClockSweep(+0);

            if (ext != null)
            {
                ext.ClockHalf();
            }
        }

        private void ClockQuad()
        {
            sq1.Envelope.Clock();
            sq2.Envelope.Clock();
            noi.Envelope.Clock();

            tri.ClockLinearCounter();

            if (ext != null)
            {
                ext.ClockQuad();
            }
        }

        private void Sample()
        {
            gameSystem.Audio.Render(ext != null
                ? Mixer.MixSamples(sq1.Render(), sq2.Render(), tri.Render(), noi.Render(), dmc.Render(), ext.Render())
                : Mixer.MixSamples(sq1.Render(), sq2.Render(), tri.Render(), noi.Render(), dmc.Render()));
        }

        public void Hook(ChannelExt external)
        {
            ext = external;
        }
    }
}
