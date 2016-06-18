using Beta.Platform.Audio;

namespace Beta.SuperFamicom
{
    public sealed class AudioParameterProvider : IAudioParameterProvider
    {
        public AudioParameters GetValue()
        {
            return new AudioParameters(32000, 2, 1f);
        }
    }
}
