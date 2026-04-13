using ExtenderApp.Serialization.Abstractions;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 自定义字典格式化器类，继承自 <see cref="InterfaceDictionaryFormatter{TKey, TValue, TDictionary}"/>。
    /// </summary>
    /// <typeparam name="TKey">字典的键类型。</typeparam>
    /// <typeparam name="TValue">字典的值类型。</typeparam>
    /// <typeparam name="TDictionary">字典类型，必须实现 <see cref="IDictionary{TKey, TValue}"/> 接口且拥有无参构造函数。</typeparam>
    internal sealed class CustomizeDictionaryFormatter<TKey, TValue, TDictionary> : InterfaceDictionaryFormatter<TKey, TValue, TDictionary> where TDictionary : IDictionary<TKey, TValue>, new()
    {
        /// <summary>
        /// 集合辅助类实例。
        /// </summary>
        private CollectionHelpers<TDictionary> _collectionHelpers;

        /// <summary>
        /// 初始化 <see cref="CustomizeDictionaryFormatter{TKey, TValue, TDictionary}"/> 类的新实例。
        /// </summary>
        /// <param name="resolver">二进制格式化器解析器。</param>
        /// <param name="binarybufferConvert">二进制读取转换器。</param>
        /// <param name="binarybufferConvert">二进制读取转换器。</param>
        /// <param name="options">二进制选项。</param>
        public CustomizeDictionaryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _collectionHelpers = new CollectionHelpers<TDictionary>();
        }

        /// <summary>
        /// 创建一个新的字典实例。
        /// </summary>
        /// <param name="count">字典的初始容量。</param>
        /// <returns>返回一个新的字典实例。</returns>
        protected override sealed TDictionary Create(int count)
        {
            return _collectionHelpers.CreateCollection(count);
        }
    }
}