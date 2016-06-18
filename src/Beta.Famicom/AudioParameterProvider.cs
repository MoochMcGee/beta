using Beta.Platform.Audio;

namespace Beta.Famicom
{
    public sealed class AudioParameterProvider : IAudioParameterProvider
    {
        public AudioParameters GetValue()
        {
            const float Ratio = ((341f * 262f) * 60f) / (236250000f / 44f);

            return new AudioParameters(48000, 1, Ratio);
        }
    }
}
