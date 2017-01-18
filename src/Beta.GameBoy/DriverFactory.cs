using Beta.GameBoy.APU;
using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.Messaging;
using Beta.GameBoy.PPU;
using Beta.Platform;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.GameBoy
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly CartridgeConnector cartridgeConnector;
        private readonly Container container;

        public DriverFactory(Container container, Apu apu, Cpu cpu, Pad pad, Ppu ppu, Tma tma, CartridgeConnector cartridge, ISignalBroker broker)
        {
            this.container = container;
            this.cartridgeConnector = cartridge;

            broker.Link(apu);
            broker.Link(cpu);
            broker.Link(pad);
            broker.Link(ppu);
            broker.Link<ClockSignal>(tma);
            broker.Link<ResetDividerSignal>(tma);
        }

        public IDriver Create(byte[] binary)
        {
            cartridgeConnector.InsertCartridge(binary);

            return container.GetInstance<Driver>();
        }
    }
}
