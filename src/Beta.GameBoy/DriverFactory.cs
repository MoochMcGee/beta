using Beta.GameBoy.APU;
using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.GameBoy
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly CartridgeConnector cartridgeConnector;
        private readonly Container container;

        public DriverFactory(Container container, Apu apu, Cpu cpu, Pad pad, Ppu ppu, Tma tma, CartridgeConnector cartridge, ISubscriptionBroker broker)
        {
            this.container = container;
            this.cartridgeConnector = cartridge;

            broker.Subscribe(apu);
            broker.Subscribe(cpu);
            broker.Subscribe(pad);
            broker.Subscribe(ppu);
            broker.Subscribe(tma);
        }

        public IDriver Create(byte[] binary)
        {
            cartridgeConnector.InsertCartridge(binary);

            return container.GetInstance<Driver>();
        }
    }
}
