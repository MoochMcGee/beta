using System.Collections.Generic;
using Beta.Famicom.Boards;
using Beta.Famicom.Database;
using Beta.Famicom.Formats;
using Beta.Famicom.Memory;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Packaging;
using Beta.Platform.Video;
using SimpleInjector;

namespace Beta.Famicom
{
    public sealed class Package : IPackage
    {
        public string Name => "Famicom";

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return new FileExtension("nes");
            }
        }

        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<ICartridgeFactory, CartridgeFactory>();
            container.RegisterSingleton<IDatabase, DatabaseService>();
            container.RegisterSingleton<IMemoryFactory, MemoryFactory>();
            container.RegisterSingleton<IBoardManager, BoardManager>();
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IPowerButton, PowerButton>();
            container.RegisterSingleton<IResetButton, ResetButton>();
            container.RegisterSingleton<IAudioParameterProvider, AudioParameterProvider>();
            container.RegisterSingleton<IVideoParameterProvider, VideoParameterProvider>();

            container.RegisterSingleton<GameSystem>();
        }
    }
}
