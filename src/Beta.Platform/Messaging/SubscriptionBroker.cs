using SimpleInjector;

namespace Beta.Platform.Messaging
{
    public sealed class SubscriptionBroker : ISubscriptionBroker
    {
        private readonly Container container;

        public SubscriptionBroker(Container container)
        {
            this.container = container;
        }

        public void Subscribe<T>(IConsumer<T> consumer)
        {
            var producer = container.GetInstance<IProducer<T>>();
            producer.Subscribe(consumer);
        }
    }
}
