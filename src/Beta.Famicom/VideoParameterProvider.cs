using Beta.Platform.Video;

namespace Beta.Famicom
{
    public sealed class VideoParameterProvider : IVideoParameterProvider
    {
        public VideoParameters GetValue()
        {
            return new VideoParameters(256, 240);
        }
    }
}
