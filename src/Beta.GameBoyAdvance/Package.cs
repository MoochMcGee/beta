using Beta.GameBoyAdvance.Memory;
using Beta.Platform.Core;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.GameBoyAdvance
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();

            container.RegisterSingleton<Registers>();
            container.RegisterSingleton<BIOS>();
            container.RegisterSingleton<ERAM>();
            container.RegisterSingleton<IRAM>();
            container.RegisterSingleton<MMIO>();
            container.RegisterSingleton<ORAM>();
            container.RegisterSingleton<PRAM>();
            container.RegisterSingleton<VRAM>();
        }
    }
}
