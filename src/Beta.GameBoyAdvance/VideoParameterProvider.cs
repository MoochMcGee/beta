using Beta.Platform.Video;

namespace Beta.GameBoyAdvance
{
    public sealed class VideoParameterProvider : IVideoParameterProvider
    {
        public VideoParameters GetValue()
        {
            return new VideoParameters(240, 160);
        }
    }
}
