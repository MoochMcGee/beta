using Beta.Platform.Core;
using Beta.SuperFamicom.CPU;
using Beta.SuperFamicom.Memory;
using Beta.SuperFamicom.PPU;
using Beta.SuperFamicom.SMP;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.SuperFamicom
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();

            container.RegisterSingleton<State>();
            container.RegisterSingleton<BusA>();
            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<Dsp>();
            container.RegisterSingleton<Ppu>();
            container.RegisterSingleton<PSRAM>();
            container.RegisterSingleton<Smp>();
            container.RegisterSingleton<WRAM>();
        }
    }
}
