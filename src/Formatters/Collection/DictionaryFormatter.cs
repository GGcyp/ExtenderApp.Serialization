using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// DictionaryFormatter 类，用于实现 InterfaceDictionaryFormatter 接口，提供对字典数据的格式化功能。
    /// </summary>
    /// <typeparam name="TKey">字典键的类型。</typeparam>
    /// <typeparam name="TValue">字典值的类型。</typeparam>
    public sealed class DictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, Dictionary<TKey, TValue>> where TKey : notnull
    {
        public DictionaryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed Dictionary<TKey, TValue> Create(int count)
        {
            return new Dictionary<TKey, TValue>(count);
        }
    }
}