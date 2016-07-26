using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Database;
using Beta.Famicom.Formats;
using Beta.Famicom.Input;
using Beta.Famicom.Memory;
using Beta.Famicom.PPU;
using Beta.Platform.Core;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.Famicom
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IBoardFactory, BoardFactory>();
            container.RegisterSingleton<ICartridgeFactory, CartridgeFactory>();
            container.RegisterSingleton<IDatabase, DatabaseService>();
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();
            container.RegisterSingleton<IJoypadFactory, JoypadFactory>();
            container.RegisterSingleton<IMemoryFactory, MemoryFactory>();

            container.RegisterSingleton<Driver>();
            container.RegisterSingleton<InputConnector>();
            container.RegisterSingleton<Mixer>();
            container.RegisterSingleton<Noi>();
            container.RegisterSingleton<R2A03>();
            container.RegisterSingleton<R2A03Bus>();
            container.RegisterSingleton<R2A03MemoryMap>();
            container.RegisterSingleton<R2A03State>();
            container.RegisterSingleton<R2A03Registers>();
            container.RegisterSingleton<R2C02>();
            container.RegisterSingleton<R2C02Bus>();
            container.RegisterSingleton<R2C02Registers>();
            container.RegisterSingleton<Sq1>();
            container.RegisterSingleton<Sq2>();
            container.RegisterSingleton<State>();
            container.RegisterSingleton<Tri>();
        }
    }
}
