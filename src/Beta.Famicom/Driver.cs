using Beta.Famicom.CPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom
{
    public sealed class Driver : IDriver
    {
        private readonly IAudioBackend audio;
        private readonly R2A03State r2a03;

        public Driver(IAudioBackend audio, R2A03State r2a03)
        {
            this.audio = audio;
            this.r2a03 = r2a03;
        }

        public void Main()
        {
            R6502.ResetHard(r2a03.r6502);

            while (true)
            {
                R2A03.Update(r2a03);
            }
        }
    }
}
