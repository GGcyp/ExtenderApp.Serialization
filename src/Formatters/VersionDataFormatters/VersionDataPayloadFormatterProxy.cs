using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 将 <see cref="VersionDataFormatterManager{T}"/> 对载荷类型 <typeparamref name="T"/> 的序列化能力暴露为 <see cref="BinaryFormatter{T}"/>，
    /// 以便解析器在注册表中对 <c>typeof(T)</c> 与 <c>typeof(VersionData&lt;T&gt;)</c> 分别返回代理与真实管理器实例。
    /// </summary>
    internal sealed class VersionDataPayloadFormatterProxy<T> : BinaryFormatter<T>
    {
        private readonly VersionDataFormatterManager<T> _manager;

        /// <summary>
        /// 使用已构造的版本管理器创建载荷代理格式化器。
        /// </summary>
        public VersionDataPayloadFormatterProxy(VersionDataFormatterManager<T> manager)
        {
            _manager = manager;
        }

        /// <inheritdoc />
        public override int DefaultLength => _manager.DefaultLength;

        /// <inheritdoc />
        public override void Serialize(ref SpanWriter<byte> writer, T value) => _manager.Serialize(ref writer, value);

        /// <inheritdoc />
        public override void Serialize(ref BinaryWriterAdapter writer, T value) => _manager.Serialize(ref writer, value);

        /// <inheritdoc />
        public override T Deserialize(ref SpanReader<byte> reader) => _manager.Deserialize(ref reader).Data;

        /// <inheritdoc />
        public override T Deserialize(ref BinaryReaderAdapter reader) => _manager.Deserialize(ref reader).Data;

        /// <inheritdoc />
        public override long GetLength(T value) => _manager.GetLength(value);
    }
}
