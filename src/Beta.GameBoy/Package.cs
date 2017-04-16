using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.PPU;
using Beta.Platform;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.GameBoy
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();

            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<MemoryMap>();
            container.RegisterSingleton<Pad>();
            container.RegisterSingleton<Ppu>();
            container.RegisterSingleton<State>();
        }
    }
}
