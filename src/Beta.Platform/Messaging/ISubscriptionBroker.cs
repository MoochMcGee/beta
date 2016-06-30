namespace Beta.Platform.Messaging
{
    public interface ISubscriptionBroker
    {
        void Subscribe<T>(IConsumer<T> consumer);
    }
}
