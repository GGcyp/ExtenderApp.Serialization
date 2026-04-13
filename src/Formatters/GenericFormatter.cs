using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 一个通用的二进制格式化器，用于依赖注入中的二进制格式化器生成。
    /// </summary>
    /// <typeparam name="T">需要被二进制格式化的类型。</typeparam>
    internal sealed class GenericFormatter<T> : ResolverFormatter<T>
    {
        private BinaryFormatter<T> _innerFormatter;

        public GenericFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _innerFormatter = resolver.GetFormatter<T>();
        }

        public override sealed int DefaultLength => _innerFormatter.DefaultLength;

        public override sealed void Serialize(ref BinaryWriterAdapter writer, T value)
        {
            _innerFormatter.Serialize(ref writer, value);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, T value)
        {
            _innerFormatter.Serialize(ref writer, value);
        }

        public override sealed T Deserialize(ref BinaryReaderAdapter reader)
        {
            return _innerFormatter.Deserialize(ref reader);
        }

        public override sealed T Deserialize(ref SpanReader<byte> reader)
        {
            return _innerFormatter.Deserialize(ref reader);
        }

        public override sealed long GetLength(T value)
        {
            return _innerFormatter.GetLength(value);
        }
    }
}