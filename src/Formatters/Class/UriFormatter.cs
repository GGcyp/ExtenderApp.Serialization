using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// Uri 类型的二进制格式化器。 通过将 Uri 转换为字符串进行序列化和反序列化，支持 null 值。 在反序列化时，如果字符串为空或 null，则返回 null Uri。
    /// </summary>
    internal sealed class UriFormatter : ResolverFormatter<Uri>
    {
        private readonly BinaryFormatter<string> _string;

        public UriFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
        }

        public override sealed Uri Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return null!;
            return new Uri(result);
        }

        public override sealed Uri Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return null!;
            return new Uri(result);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Uri value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value?.ToString() ?? string.Empty);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Uri value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value?.ToString() ?? string.Empty);
        }

        public override sealed long GetLength(Uri value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _string.GetLength(value.ToString());
        }
    }
}