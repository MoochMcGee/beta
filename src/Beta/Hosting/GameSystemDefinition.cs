using Beta.Platform.Configuration;
using SimpleInjector.Packaging;

namespace Beta.Hosting
{
    public sealed class GameSystemDefinition
    {
        public IPackage Package { get; set; }

        public ConfigurationFile File { get; set; }
    }
}
