using System.Collections.Generic;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Packaging;
using Beta.Platform.Video;
using SimpleInjector;

namespace Beta.GameBoy
{
    public sealed class Package : IPackage
    {
        public string Name => "Game Boy";

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return new FileExtension("gb");
                yield return new FileExtension("gbc");
            }
        }

        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IPowerButton, PowerButton>();
            container.RegisterSingleton<IResetButton, DefaultResetButton>();
            container.RegisterSingleton<IAudioParameterProvider, AudioParameterProvider>();
            container.RegisterSingleton<IVideoParameterProvider, VideoParameterProvider>();
        }
    }
}
