using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        public const int PHASE = 0;
        public const int DELAY = 0;

        public void Hook(ChannelExt ext)
        {
        }

        public class Channel
        {
            public Timing Timing;
            public int Frequency;
        }

        public class ChannelExt
        {
            public virtual void Initialize()
            {
            }

            public virtual short Render()
            {
                return 0;
            }
        }
    }
}
