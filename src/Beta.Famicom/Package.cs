using Beta.Famicom.APU;
using Beta.Famicom.PPU;
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
            container.RegisterSingleton<IDriverFactory, DriverFactory>();

            container.RegisterSingleton<Driver>();
            container.RegisterSingleton<State>();
            container.RegisterSingleton<Mixer>();
            container.RegisterSingleton<R2C02>();
        }
    }
}
