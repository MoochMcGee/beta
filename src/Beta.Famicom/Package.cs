using System.Collections.Generic;
using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Database;
using Beta.Famicom.Formats;
using Beta.Famicom.Input;
using Beta.Famicom.Memory;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
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
            container.RegisterSingleton<IEmulationLoop, EmulationLoop>();
            container.RegisterSingleton<IMemoryFactory, MemoryFactory>();
            container.RegisterSingleton<IBoardFactory, BoardFactory>();
            container.RegisterSingleton<IJoypadFactory, JoypadFactory>();
            container.RegisterSingleton<IGameSystem, GameSystem>();
            container.RegisterSingleton<IGameSystemFactory, GameSystemFactory>();

            container.RegisterSingleton<GameSystem>();
            container.RegisterSingleton<R2A03>();
            container.RegisterSingleton<R2A03Bus>();
            container.RegisterSingleton<R2C02>();
            container.RegisterSingleton<R2C02Bus>();

            container.Register(typeof(IProducer<>), typeof(Producer<>), Lifestyle.Singleton);

            container.RegisterSingleton<ISubscriptionBroker, SubscriptionBroker>();
            container.RegisterSingleton<IAudioBackend, AudioBackend>();
            container.RegisterSingleton<IAudioParameterProvider, AudioParameterProvider>();
            container.RegisterSingleton<IVideoBackend, VideoBackend>();
            container.RegisterSingleton<IVideoParameterProvider, VideoParameterProvider>();
            container.RegisterInitializer<IAudioBackend>(e => e.Initialize());
            container.RegisterInitializer<IVideoBackend>(e => e.Initialize());
        }
    }
}
