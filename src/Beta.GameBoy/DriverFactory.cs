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
        private readonly Container container;
        private readonly ICartridgeConnector cartridgeConnector;
        private readonly ISubscriptionBroker broker;

        public DriverFactory(Container container, ICartridgeConnector cartridge, ISubscriptionBroker broker)
        {
            this.container = container;
            this.cartridgeConnector = cartridge;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            broker.Subscribe(container.GetInstance<Apu>());
            broker.Subscribe(container.GetInstance<Cpu>());
            broker.Subscribe(container.GetInstance<Pad>());
            broker.Subscribe(container.GetInstance<Ppu>());
            broker.Subscribe(container.GetInstance<Tma>());

            cartridgeConnector.InsertCartridge(binary);

            var addressSpace = container.GetInstance<IAddressSpace>();
            var bios = container.GetInstance<BIOS>();
            var vram = container.GetInstance<VRAM>();
            var wram = container.GetInstance<WRAM>();
            var hram = container.GetInstance<HRAM>();
            var  oam = container.GetInstance< OAM>();

            addressSpace.Map(0x0000, 0x7fff, cartridgeConnector.Read, cartridgeConnector.Write);
            addressSpace.Map(0x8000, 0x9fff, vram.Read, vram.Write);
            addressSpace.Map(0xa000, 0xbfff, cartridgeConnector.Read, cartridgeConnector.Write);
            addressSpace.Map(0xc000, 0xfdff, wram.Read, wram.Write);
            addressSpace.Map(0xfe00, 0xfe9f,  oam.Read,  oam.Write);
            addressSpace.Map(0xff80, 0xfffe, hram.Read, hram.Write);

            return container.GetInstance<Driver>();
        }
    }
}
