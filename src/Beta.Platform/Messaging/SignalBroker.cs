using SimpleInjector;

namespace Beta.Platform.Messaging
{
    public sealed class SignalBroker : ISignalBroker
    {
        private readonly Container container;

        public SignalBroker(Container container)
        {
            this.container = container;
        }

        public void Link<T>(IConsumer<T> consumer)
        {
            var producer = container.GetInstance<IProducer<T>>();
            producer.Subscribe(consumer);
        }
    }
}
