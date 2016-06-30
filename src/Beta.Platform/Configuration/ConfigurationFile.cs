using Newtonsoft.Json;

namespace Beta.Platform.Configuration
{
    public sealed class ConfigurationFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }

        [JsonProperty("audio")]
        public AudioConfiguration Audio { get; set; }

        [JsonProperty("video")]
        public VideoConfiguration Video { get; set; }

        [JsonProperty("inputs")]
        public InputConfiguration[] Inputs { get; set; }
    }
}
