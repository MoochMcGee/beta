using Beta.Famicom.CPU;
using Beta.Platform;

namespace Beta.Famicom
{
    public sealed class Driver : IDriver
    {
        private readonly R2A03 r2a03;

        public Driver(R2A03 r2a03)
        {
            this.r2a03 = r2a03;
        }

        public void Main()
        {
            r2a03.ResetHard();

            while (true)
            {
                r2a03.Update();
            }
        }
    }
}
