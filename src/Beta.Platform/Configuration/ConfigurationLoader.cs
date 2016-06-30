using System.IO;
using Newtonsoft.Json;

namespace Beta.Platform.Configuration
{
    public static class ConfigurationLoader
    {
        public static ConfigurationFile Load(string fileName)
        {
            var textReader = File.OpenText(fileName);
            var jsonReader = new JsonTextReader(textReader);
            var jsonSerializer = JsonSerializer.Create();

            return jsonSerializer.Deserialize<ConfigurationFile>(jsonReader);
        }
    }
}
