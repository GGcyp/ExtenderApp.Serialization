using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters.Collection
{
    internal sealed class ByteArrayFormatter : ResolverFormatter<byte[]>
    {
        private readonly BinaryFormatter<int> _int;

        public ByteArrayFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override sealed byte[] Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return Array.Empty<byte>();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new Exception("无法反序列化数据，数据类型不匹配。");
            }

            var len = _int.Deserialize(ref reader);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = new byte[len];
            reader.TryRead(array);
            return array;
        }

        public override sealed byte[] Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Array.Empty<byte>();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new Exception("无法反序列化数据，数据类型不匹配。");
            }

            var len = _int.Deserialize(ref reader);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = new byte[len];
            reader.Read(array.AsSpan());
            return array;
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, byte[] value)
        {
            if (value == null || value == Array.Empty<byte>())
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            writer.Write(value);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, byte[] value)
        {
            if (value == null || value == Array.Empty<byte>())
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            writer.Write(value);
        }

        public override sealed long GetLength(byte[] value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Length);
            result += value.Length;
            return result;
        }
    }
}