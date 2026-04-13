using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary> StackFormatter 类是 CollectionFormatter 类的泛型实现，用于格式化 Stack<TLinkClient> 集合。 </summary> <typeparam name="T">Stack<TLinkClient> 集合中元素的类型。</typeparam>
    internal sealed class StackFormatter<T> : InterfaceEnumerableFormatter<T, Stack<T>>
    {
        public StackFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(Stack<T> collection, T value)
        {
            collection.Push(value);
        }

        protected override sealed Stack<T> Create(int count)
        {
            return new Stack<T>(count);
        }
    }
}