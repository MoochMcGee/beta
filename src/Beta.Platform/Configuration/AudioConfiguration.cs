using Newtonsoft.Json;

namespace Beta.Platform.Configuration
{
    public sealed class AudioConfiguration
    {
        [JsonProperty("channels")]
        public int Channels { get; set; }

        [JsonProperty("sample-rate")]
        public int SampleRate { get; set; }
    }
}
