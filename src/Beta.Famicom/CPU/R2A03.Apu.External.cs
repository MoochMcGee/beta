namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        public void Hook(ChannelExt ext)
        {
        }

        public class ChannelExt
        {
            public virtual void ClockHalf()
            {
            }

            public virtual void ClockQuad()
            {
            }

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
