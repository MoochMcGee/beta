using Beta.Platform.Audio;

namespace Beta.GameBoy
{
    public sealed class AudioParameterProvider : IAudioParameterProvider
    {
        public AudioParameters GetValue()
        {
            return new AudioParameters(48000, 2, 1f);
        }
    }
}
