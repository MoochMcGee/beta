using Beta.Platform.Audio;
using Beta.Platform.Video;

namespace Beta.Platform.Core
{
    public delegate byte Peek(uint address);

    public delegate void Poke(uint address, byte data);

    public interface IGameSystem
    {
        IAudioBackend Audio { get; set; }

        IVideoBackend Video { get; set; }

        void Emulate();

        void Initialize();

        // void LoadGame(byte[] binary);
    }
}
