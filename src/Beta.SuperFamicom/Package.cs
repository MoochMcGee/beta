using Beta.Platform.Core;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Beta.SuperFamicom
{
    public sealed class Package : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IPowerButton, PowerButton>();
            container.RegisterSingleton<IResetButton, ResetButton>();
        }
    }
}
