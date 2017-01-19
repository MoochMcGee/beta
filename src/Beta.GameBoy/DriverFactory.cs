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

        public DriverFactory(Container container, Apu apu, Cpu cpu, Pad pad, Ppu ppu, Tma tma, CartridgeConnector cartridge, SignalBroker broker)
        {
            this.container = container;
            this.cartridgeConnector = cartridge;

            broker.Link<ClockSignal>(apu.Consume);
            broker.Link<InterruptSignal>(cpu.Consume);
            broker.Link<FrameSignal>(pad.Consume);
            broker.Link<ClockSignal>(ppu.Consume);
            broker.Link<ClockSignal>(tma.Consume);
            broker.Link<ResetDividerSignal>(tma.Consume);
        }

        public IDriver Create(byte[] binary)
        {
            cartridgeConnector.InsertCartridge(binary);

            return container.GetInstance<Driver>();
        }
    }
}
