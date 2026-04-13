using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 表示一个内部类 HashSetFormatter，它是 CollectionFormatter 的泛型子类，用于格式化 HashSet<TLinkClient> 集合。
    /// </summary>
    /// <typeparam name="T">HashSet<TLinkClient> 中元素的类型。</typeparam>
    internal sealed class HashSetFormatter<T> : InterfaceCollectionFormatter<T, HashSet<T>>
    {
        public HashSetFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(HashSet<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override sealed HashSet<T> Create(int count)
        {
            return new HashSet<T>(count);
        }
    }
}