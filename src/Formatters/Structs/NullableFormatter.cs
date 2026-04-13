using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 静态可空格式化器类，用于处理可空的结构体类型的序列化和反序列化。
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    public sealed class NullableFormatter<T> : ResolverFormatter<T?>
        where T : struct
    {
        /// <summary>
        /// 底层格式化器，用于处理非可空结构体类型的序列化和反序列化。
        /// </summary>
        private readonly BinaryFormatter<T> _formatter;

        public NullableFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<T>();
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, T? value)
        {
            if (value.HasValue)
            {
                _formatter.Serialize(ref writer, value.Value);
            }
            else
            {
                WriteNil(ref writer);
            }
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, T? value)
        {
            if (value.HasValue)
            {
                _formatter.Serialize(ref writer, value.Value);
            }
            else
            {
                WriteNil(ref writer);
            }
        }

        public override sealed T? Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return null;
            }
            return _formatter.Deserialize(ref reader);
        }

        public override sealed T? Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null;
            }
            return _formatter.Deserialize(ref reader);
        }

        public override sealed long GetLength(T? value)
        {
            return value.HasValue ? _formatter.GetLength(value.Value) : 1;
        }
    }
}