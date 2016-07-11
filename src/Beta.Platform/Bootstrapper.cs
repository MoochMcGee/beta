using System;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;
using Beta.Platform.Video;
using SimpleInjector;

namespace Beta.Platform
{
    public static class Bootstrapper
    {
        public static Container Bootstrap(IntPtr handle)
        {
            var container = new Container();
            container.Register(typeof(IProducer<>), typeof(Producer<>), Lifestyle.Singleton);

            container.RegisterSingleton<IHwndProvider>(new HwndProvider(handle));

            container.RegisterSingleton<ISubscriptionBroker, SubscriptionBroker>();
            container.RegisterSingleton<IAudioBackend, AudioBackend>();
            container.RegisterSingleton<IVideoBackend, VideoBackend>();
            container.RegisterInitializer<IVideoBackend>(e => e.Initialize());

            return container;
        }
    }
}
