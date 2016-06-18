using Beta.Platform.Video;

namespace Beta.SuperFamicom
{
    public sealed class VideoParameterProvider : IVideoParameterProvider
    {
        public VideoParameters GetValue()
        {
            return new VideoParameters(256, 240);
        }
    }
}
