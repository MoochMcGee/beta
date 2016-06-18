using System;

namespace Beta.Platform.Video
{
    public interface IVideoBackend : IDisposable
    {
        int[] GetRaster(int line);

        void Initialize();

        void Render();
    }
}
