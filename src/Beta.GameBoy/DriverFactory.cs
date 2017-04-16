using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
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
        private readonly Container container;

        public DriverFactory(Container container)
        {
            this.container = container;

            var broker = container.GetInstance<SignalBroker>();
            broker.Link<InterruptSignal>(container.GetInstance<Cpu>().Consume);
            broker.Link<FrameSignal>(container.GetInstance<Pad>().Consume);
            broker.Link<ClockSignal>(container.GetInstance<Ppu>().Consume);
        }

        public IDriver Create(byte[] binary)
        {
            var state = container.GetInstance<State>();
            var board = BoardFactory.Create(binary);

            CartridgeConnector.InsertCartridge(state, board);

            return container.GetInstance<Driver>();
        }
    }
}
