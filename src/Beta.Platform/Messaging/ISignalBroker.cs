namespace Beta.Platform.Messaging
{
    public interface ISignalBroker
    {
        void Link<T>(IConsumer<T> consumer);
    }
}
