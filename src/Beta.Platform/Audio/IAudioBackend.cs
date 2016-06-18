using System;

namespace Beta.Platform.Audio
{
    public interface IAudioBackend : IDisposable
    {
        void Initialize();

        void Render(int sample);
    }
}
