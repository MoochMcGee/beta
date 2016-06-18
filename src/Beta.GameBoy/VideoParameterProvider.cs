using Beta.Platform.Video;

namespace Beta.GameBoy
{
    public sealed class VideoParameterProvider : IVideoParameterProvider
    {
        public VideoParameters GetValue()
        {
            return new VideoParameters(160, 144);
        }
    }
}
