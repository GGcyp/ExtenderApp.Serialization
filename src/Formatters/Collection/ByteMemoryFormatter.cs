using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// ByteMemoryFormatter 是一个专门用于序列化和反序列化 <see cref="Memory{byte}"/> 类型的二进制格式化器。 它通过写入长度前缀和字节数据来实现高效的二进制表示， 同时支持空值处理以优化性能和内存使用。 适用于需要在二进制格式中传输或存储字节数据的场景。
    /// </summary>
    internal sealed class ByteMemoryFormatter : ResolverFormatter<Memory<byte>>
    {
        private readonly BinaryFormatter<int> _int;

        public ByteMemoryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override sealed Memory<byte> Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<byte>.Empty;
            }

            if (TryReadMapHeader(ref reader))
            {
                ThrowOperationException("Memory<Byte> 数据格式错误，无法反序列化");
            }

            int length = _int.Deserialize(ref reader);
            byte[] buffer = new byte[length];
            reader.TryRead(buffer);
            return new Memory<byte>(buffer);
        }

        public override sealed Memory<byte> Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<byte>.Empty;
            }

            if (TryReadMapHeader(ref reader))
            {
                ThrowOperationException("Memory<Byte> 数据格式错误，无法反序列化");
            }

            int length = _int.Deserialize(ref reader);
            byte[] buffer = new byte[length];
            reader.TryRead(buffer);
            return new Memory<byte>(buffer);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Memory<byte> value)
        {
            if (value.IsEmpty || value.Length == 0)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMapHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            writer.Write(value.Span);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Memory<byte> value)
        {
            if (value.IsEmpty || value.Length == 0)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMapHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            writer.Write(value.Span);
        }

        public override sealed long GetLength(Memory<byte> value)
        {
            if (value.IsEmpty)
            {
                return NilLength;
            }

            return _int.GetLength(value.Length) + value.Length;
        }
    }
}