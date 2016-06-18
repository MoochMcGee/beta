using Beta.Platform.Audio;

namespace Beta.GameBoyAdvance
{
    public sealed class AudioParameterProvider : IAudioParameterProvider
    {
        public AudioParameters GetValue()
        {
            return new AudioParameters(48000, 2, 1f);
        }
    }
}
