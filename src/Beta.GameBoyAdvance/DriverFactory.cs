using Beta.Platform.Core;
using SimpleInjector;

namespace Beta.GameBoyAdvance
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;

        public DriverFactory(Container container)
        {
            this.container = container;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = container.GetInstance<Driver>();
            driver.LoadGame(binary);

            return driver;
        }
    }
}
