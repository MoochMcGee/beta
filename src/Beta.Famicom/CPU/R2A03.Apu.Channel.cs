using Beta.Platform;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        public const int DELAY = 13125;
        public const int PHASE = 352;

        public class Channel
        {
            protected Timing Timing;
            protected int Frequency;

            public virtual void Initialize()
            {
            }
        }
    }
}
