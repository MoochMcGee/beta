using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Video;
using Beta.R6502;

namespace Beta.Famicom
{
    public sealed class Driver : IDriver
    {
        private readonly IAudioBackend audio;
        private readonly IVideoBackend video;

        public Driver(IAudioBackend audio, IVideoBackend video)
        {
            this.audio = audio;
            this.video = video;
        }

        public void main()
        {
            var state = new State();

            SpUnit.evaluationReset(state.r2c02);
            SpUnit.initializeSprite(state.r2c02);

            while (true)
            {
                runForOneFrame(state);
            }
        }

        private void runForOneFrame(State e)
        {
            do
            {
                R2A03.tick(e, audio, video);
            }
            while (e.r2c02.h != 0 && e.r2c02.v != 0);

            InputConnector.update();
        }
    }
}
