using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;

namespace Beta.GameBoyAdvance
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly IAudioBackend audio;
        private readonly IVideoBackend video;

        public DriverFactory(IAudioBackend audio, IVideoBackend video)
        {
            this.audio = audio;
            this.video = video;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = new Driver(audio, video);
            driver.LoadGame(binary);

            return driver;
        }
    }
}
