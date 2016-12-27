using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.PPU;
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
            container.RegisterSingleton<MemoryMap>();

            container.RegisterSingleton<DmaController>();
            container.RegisterSingleton<TimerController>();

            container.RegisterSingleton<Apu>();
            container.RegisterSingleton<Cpu>();
            container.RegisterSingleton<Pad>();
            container.RegisterSingleton<Ppu>();

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
