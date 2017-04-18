using Beta.Famicom.Boards;
using Beta.Famicom.Input;
using Beta.Famicom.Memory;
using Beta.Platform;
using SimpleInjector;

namespace Beta.Famicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;

        public DriverFactory(Container container)
        {
            this.container = container;
        }

        public IDriver create(byte[] binary)
        {
            InputConnector.connectJoypad1(JoypadFactory.create(0));
            InputConnector.connectJoypad2(JoypadFactory.create(1));

            var board = BoardFactory.getBoard(binary);

            CartridgeConnector.insertCartridge(board);

            return container.GetInstance<Driver>();
        }
    }
}
