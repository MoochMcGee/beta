namespace Beta.Platform.Messaging
{
    public interface IProducer<T>
    {
        void Produce(T e);

        void Subscribe(Consumer<T> subscriber);
    }
}
