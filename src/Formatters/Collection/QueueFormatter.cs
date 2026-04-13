using ExtenderApp.Serialization.Abstractions;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 队列格式化器类，继承自 <see cref="InterfaceEnumerableFormatter{T, Queue{T}}"/>。
    /// </summary>
    /// <typeparam name="T">队列中元素的类型。</typeparam>
    internal sealed class QueueFormatter<T> : InterfaceEnumerableFormatter<T, Queue<T>>
    {
        public QueueFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(Queue<T> collection, T value)
        {
            collection.Enqueue(value);
        }

        protected override sealed Queue<T> Create(int count)
        {
            return new Queue<T>(count);
        }
    }
}