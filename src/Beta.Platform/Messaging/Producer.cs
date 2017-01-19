using System.Collections.Generic;

namespace Beta.Platform.Messaging
{
    public sealed class Producer<T> : IProducer<T>
    {
        private List<Consumer<T>> consumers = new List<Consumer<T>>();

        public void Produce(T e)
        {
            int count = consumers.Count;

            for (int i = 0; i < count; i++)
            {
                consumers[i](e);
            }
        }

        public void Subscribe(Consumer<T> consumer)
        {
            consumers.Add(consumer);
        }
    }
}
