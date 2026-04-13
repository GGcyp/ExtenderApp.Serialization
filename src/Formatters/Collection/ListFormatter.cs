using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 列表格式化器：专门处理 <see cref="List{T}"/> 类型的二进制序列化和反序列化。 通过继承自 <see cref="InterfaceCollectionFormatter{T, List{T}}"/>，它实现了针对列表的特定行为，
    /// 包括创建具有适当容量的列表实例和将元素添加到列表中。 该格式化器利用二进制格式化器解析器来获取元素类型的格式化器，从而支持对列表中元素的正确序列化和反序列化。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ListFormatter<T> : InterfaceCollectionFormatter<T, List<T>>
    {
        public ListFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Add(List<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override List<T> Create(int count)
        {
            return new List<T>(count);
        }
    }
}