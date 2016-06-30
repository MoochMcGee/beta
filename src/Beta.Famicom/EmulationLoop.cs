using System.Threading;
using Beta.Famicom.CPU;
using Beta.Platform;

namespace Beta.Famicom
{
    public sealed class EmulationLoop : IEmulationLoop
    {
        private readonly R2A03 r2a03;

        public EmulationLoop(R2A03 r2a03)
        {
            this.r2a03 = r2a03;
        }

        public void Main()
        {
            r2a03.ResetHard();

            try
            {
                while (true)
                {
                    r2a03.Update();
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}
