using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// char 类型的二进制格式化器。 将 char 作为 UInt16 进行序列化和反序列化， 因为 char 在 .NET 中是一个 16 位 Unicode 字符。 通过使用 UInt16 的格式化器来处理 char，可以重用现有的逻辑并确保正确处理字符数据。
    /// </summary>
    internal sealed class CharFormatter : ResolverFormatter<Char>
    {
        private readonly BinaryFormatter<UInt16> _uint16;

        public override int DefaultLength => _uint16.DefaultLength;

        public CharFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _uint16 = GetFormatter<UInt16>();
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, char value)
        {
            _uint16.Serialize(ref writer, (UInt16)value);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, char value)
        {
            _uint16.Serialize(ref writer, (UInt16)value);
        }

        public override sealed char Deserialize(ref BinaryReaderAdapter reader)
        {
            return (char)_uint16.Deserialize(ref reader);
        }

        public override sealed char Deserialize(ref SpanReader<byte> reader)
        {
            return (char)_uint16.Deserialize(ref reader);
        }

        public override sealed long GetLength(char value)
        {
            return _uint16.GetLength((UInt16)value);
        }
    }
}