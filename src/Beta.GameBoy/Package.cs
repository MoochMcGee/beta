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

            container.RegisterSingleton<Apu>();
            container.RegisterSingleton<BIOS>();
            container.RegisterSingleton<BoardFactory>();
            container.RegisterSingleton<CartridgeConnector>();
            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<HRAM>();
            container.RegisterSingleton<MemoryMap>();
            container.RegisterSingleton<Mixer>();
            container.RegisterSingleton<MMIO>();
            container.RegisterSingleton<OAM>();
            container.RegisterSingleton<Pad>();
            container.RegisterSingleton<Ppu>();
            container.RegisterSingleton<State>();
            container.RegisterSingleton<Tma>();
            container.RegisterSingleton<VRAM>();
            container.RegisterSingleton<Wave>();
            container.RegisterSingleton<WRAM>();
        }
    }
}
