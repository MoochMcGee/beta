using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Beta.Platform.Configuration
{
    public sealed class InputConfiguration
    {
        [JsonProperty("virtual-input-id")]
        public Guid VirtualInputId { get; set; }

        [JsonProperty("host-input-id")]
        public Guid HostInputId { get; set; }

        [JsonProperty("mapping")]
        public Dictionary<string, string> Mapping { get; set; }
    }
}
