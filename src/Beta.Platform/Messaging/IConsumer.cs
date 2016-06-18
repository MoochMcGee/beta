namespace Beta.Platform.Messaging
{
    public interface IConsumer<T>
    {
        void Consume(T e);
    }
}
