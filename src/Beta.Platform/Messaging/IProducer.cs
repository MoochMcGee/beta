namespace Beta.Platform.Messaging
{
    public interface IProducer<T>
    {
        void Produce(T e);

        void Subscribe(IConsumer<T> subscriber);
    }
}
