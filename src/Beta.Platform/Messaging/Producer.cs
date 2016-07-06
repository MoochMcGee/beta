using System.Collections.Generic;

namespace Beta.Platform.Messaging
{
    public sealed class Producer<T> : IProducer<T>
    {
        private List<IConsumer<T>> consumers = new List<IConsumer<T>>();

        public void Produce(T e)
        {
            int count = consumers.Count;

            for (int i = 0; i < count; i++)
            {
                consumers[i].Consume(e);
            }
        }

        public void Subscribe(IConsumer<T> consumer)
        {
            consumers.Add(consumer);
        }
    }
}
