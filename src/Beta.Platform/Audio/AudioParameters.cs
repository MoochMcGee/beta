namespace Beta.Platform.Audio
{
    public sealed class AudioParameters
    {
        public float Ratio { get; }
        public short Channels { get; }
        public int SampleRate { get; }

        public AudioParameters(int sampleRate, short channels, float ratio)
        {
            this.Ratio = ratio;
            this.Channels = channels;
            this.SampleRate = sampleRate;
        }
    }
}
