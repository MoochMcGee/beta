using System.Collections.Generic;

namespace Beta.Platform.Messaging
{
    public sealed class Producer<T> : IProducer<T>
    {
        private readonly List<IConsumer<T>> consumers;

        public Producer()
        {
            consumers = new List<IConsumer<T>>();
        }

        public void Produce(T e)
        {
            consumers.ForEach(s => s.Consume(e));
        }

        public void Subscribe(IConsumer<T> consumer)
        {
            consumers.Add(consumer);
        }
    }
}
