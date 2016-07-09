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
            container.RegisterSingleton<IDriver, Driver>();
            container.RegisterSingleton<IDriverFactory, DriverFactory>();

            container.RegisterSingleton<BoardFactory>();
            container.RegisterSingleton<Apu>();
            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<Ppu>();
            container.RegisterSingleton<Pad>();
            container.RegisterSingleton<Tma>();

            // Memory
            // 

            container.RegisterSingleton<CartridgeConnector>();
            container.RegisterSingleton<MemoryMap>();
            container.RegisterSingleton<Registers>();
            container.RegisterSingleton<BIOS>();
            container.RegisterSingleton<HRAM>();
            container.RegisterSingleton<MMIO>();
            container.RegisterSingleton<OAM>();
            container.RegisterSingleton<VRAM>();
            container.RegisterSingleton<Wave>();
            container.RegisterSingleton<WRAM>();
        }
    }
}
