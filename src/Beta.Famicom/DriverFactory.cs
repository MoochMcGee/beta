using Beta.Famicom.APU;
using Beta.Famicom.Boards;
using Beta.Famicom.Input;
using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.Famicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly SignalBroker broker;

        public DriverFactory(Container container, SignalBroker broker)
        {
            this.container = container;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            InputConnector.ConnectJoypad1(JoypadFactory.Create(0));
            InputConnector.ConnectJoypad2(JoypadFactory.Create(1));

            var mixer = container.GetInstance<Mixer>();
            var r2c02 = container.GetInstance<R2C02>();
            var board = BoardFactory.getBoard(binary);

            CartridgeConnector.InsertCartridge(board);

            broker.Link<ClockSignal>(r2c02.Consume);
            broker.Link<FrameSignal>(InputConnector.Consume);

            return container.GetInstance<Driver>();
        }
    }
}
