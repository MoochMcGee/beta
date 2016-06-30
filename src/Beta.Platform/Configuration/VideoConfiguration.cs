using Newtonsoft.Json;

namespace Beta.Platform.Configuration
{
    public sealed class VideoConfiguration
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
