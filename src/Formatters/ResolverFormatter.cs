using ExtenderApp.Serialization;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 基于解析器 <see cref="BinaryFormatterResolver"/> 的序列化/反序列化抽象基类。 提供按需获取其它类型格式化器的能力，并内置对 Nil 标记的写入与检测辅助方法。
    /// </summary>
    /// <typeparam name="T">目标序列化/反序列化的类型。</typeparam>
    public abstract class ResolverFormatter<T> : BinaryFormatter<T>
    {
        /// <summary>
        /// 格式化器解析器，用于解析并获取指定类型的 <see cref="BinaryFormatter{T}"/>。
        /// </summary>
        private readonly BinaryFormatterResolver _resolver;

        /// <summary>
        /// 使用给定的解析器初始化实例，并缓存 Nil 格式化器。
        /// </summary>
        /// <param name="resolver">格式化器解析器。</param>
        protected ResolverFormatter(BinaryFormatterResolver resolver) : base()
        {
            _resolver = resolver;
        }

        /// <summary>
        /// 从解析器获取指定类型的格式化器（若不存在则抛出）。
        /// </summary>
        /// <typeparam name="TValue">需要获取格式化器的类型。</typeparam>
        /// <returns><typeparamref name="TValue"/> 的格式化器实例。</returns>
        protected BinaryFormatter<TValue> GetFormatter<TValue>()
        {
            return _resolver.GetFormatterWithVerify<TValue>();
        }

        /// <summary>
        /// 获得指定类型个二进制格式转换器
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>格式转换器</returns>
        protected BinaryFormatter GetFormatter(Type type)
        {
            return _resolver.GetFormatter(type);
        }
    }
}