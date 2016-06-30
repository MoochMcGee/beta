using Beta.Platform.Core;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.GameBoyAdvance
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IPowerButton, PowerButton>();
            container.RegisterSingleton<IResetButton, DefaultResetButton>();
        }
    }
}
