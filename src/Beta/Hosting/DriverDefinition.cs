using Beta.Platform.Configuration;
using SimpleInjector.Packaging;

namespace Beta.Hosting
{
    public sealed class DriverDefinition
    {
        public IPackage Package { get; set; }

        public ConfigurationFile Configuration { get; set; }
    }
}
