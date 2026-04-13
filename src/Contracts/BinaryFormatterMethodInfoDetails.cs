using System.Reflection;

namespace ExtenderApp.Serialization.Abstractions
{
    /// <summary>
    /// 二进制格式化器核心方法的反射信息聚合。
    /// </summary>
    /// <remarks>
    /// 用于缓存并复用与某个具体格式化器相关的反射调用入口。 包含基于 AbstractBuffer 和基于 Span 的序列化与反序列化方法信息，以及长度计算方法信息。 这些 <see cref="MethodInfo"/> 通常来自已闭包的泛型格式化器实例方法。允许为 null，表示对应能力不支持或未提供。
    /// </remarks>
    public readonly struct BinaryFormatterMethodInfoDetails
    {
        /// <summary>
        /// 顺序缓冲写入方法的反射信息。
        /// </summary>
        public MethodInfo SerializeBuffer { get; }

        /// <summary>
        /// 栈上写入器写入方法的反射信息。
        /// </summary>
        public MethodInfo SerializeSpan { get; }

        /// <summary>
        /// 顺序缓冲读取器读取方法的反射信息。
        /// </summary>
        public MethodInfo DeserializeBuffer { get; }

        /// <summary>
        /// 栈上读取器读取方法的反射信息。
        /// </summary>
        public MethodInfo DeserializeSpan { get; }

        /// <summary>
        /// 估算长度方法的反射信息。
        /// </summary>
        public MethodInfo GetLength { get; }

        /// <summary>
        /// 获取一个值，指示是否所有方法信息均为 null。
        /// </summary>
        public bool IsEmpty => SerializeBuffer is null && SerializeSpan is null && DeserializeBuffer is null && DeserializeSpan is null && GetLength is null;

        public BinaryFormatterMethodInfoDetails(MethodInfo serializeBuffer, MethodInfo serializeSpan, MethodInfo deserializeBuffer, MethodInfo deserializeSpan, MethodInfo getLength)
        {
            SerializeBuffer = serializeBuffer;
            SerializeSpan = serializeSpan;
            DeserializeBuffer = deserializeBuffer;
            DeserializeSpan = deserializeSpan;
            GetLength = getLength;
        }
    }
}