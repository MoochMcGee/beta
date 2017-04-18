using Beta.Famicom.CPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom
{
    public sealed class Driver : IDriver
    {
        private readonly R2A03State state;
        private readonly IAudioBackend audio;

        public Driver(R2A03State state, IAudioBackend audio)
        {
            this.state = state;
            this.audio = audio;
        }

        public void Main()
        {
            R6502.resetHard(state.r6502);

            while (true)
            {
                R2A03.tick(state, audio);
            }
        }
    }
}
