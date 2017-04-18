using Beta.Platform;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.Famicom
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IDriver, Driver>();
        }
    }
}
