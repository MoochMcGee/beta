namespace Beta.Platform.Video
{
    public sealed class VideoParameters
    {
        public int Width { get; }
        public int Height { get; }

        public VideoParameters(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
