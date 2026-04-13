using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 值或列表格式化器：支持单个值或值列表的序列化；单元素时可省略列表包装以节省空间。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    internal sealed class ValueOrListFormatter<T> : InterfaceCollectionFormatter<T, ValueOrList<T>>
    {
        /// <summary>
        /// 使用解析器构造，委托基类完成元素读写。
        /// </summary>
        /// <param name="resolver">格式化器解析器。</param>
        public ValueOrListFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        /// <inheritdoc />
        protected override sealed void Add(ValueOrList<T> collection, T value)
        {
            collection.Add(value);
        }

        /// <inheritdoc />
        protected override sealed ValueOrList<T> Create(int count)
        {
            return new ValueOrList<T>(count);
        }
    }
}
