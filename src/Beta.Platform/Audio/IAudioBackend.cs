using System;

namespace Beta.Platform.Audio
{
    public interface IAudioBackend : IDisposable
    {
        void Render(int sample);
    }
}
