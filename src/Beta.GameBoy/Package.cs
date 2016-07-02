using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.PPU;
using Beta.Platform.Core;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.GameBoy
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IAddressSpace, AddressSpace>();
            container.RegisterSingleton<IBoardFactory, BoardFactory>();
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();
            container.RegisterSingleton<IResetButton, DefaultResetButton>();

            container.RegisterSingleton<Apu>();
            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<Ppu>();

            // Memory
            // 

            container.RegisterSingleton<Bios>();
            container.RegisterSingleton<Hram>();
            container.RegisterSingleton<Wram>();
        }
    }
}
