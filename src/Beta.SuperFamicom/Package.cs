using System.Collections.Generic;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Packaging;
using Beta.Platform.Video;
using SimpleInjector;

namespace Beta.SuperFamicom
{
    public sealed class Package : IPackage
    {
        public string Name => "Super Famicom";

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return new FileExtension("sfc");
            }
        }

        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IPowerButton, PowerButton>();
            container.RegisterSingleton<IResetButton, ResetButton>();
            container.RegisterSingleton<IAudioParameterProvider, AudioParameterProvider>();
            container.RegisterSingleton<IVideoParameterProvider, VideoParameterProvider>();
        }
    }
}
