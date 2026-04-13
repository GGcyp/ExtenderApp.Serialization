using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 字节块格式化器：提供对 <see cref="ByteBlock"/> 类型的二进制序列化和反序列化支持。 通过将字节块的长度和内容写入二进制流， 实现了高效的序列化机制，适用于需要处理大量字节数据的场景。
    /// </summary>
    internal sealed class ByteBlockFormatter : ResolverFormatter<ByteBlock>
    {
        private readonly BinaryFormatter<int> _int;

        public ByteBlockFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Committed);
            writer.Write(value.CommittedSpan);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, ByteBlock value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Committed);
            writer.Write(value);
        }

        public override sealed ByteBlock Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return new();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 ByteBlock 类型，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            if (length > reader.Remaining)
                throw new IndexOutOfRangeException("数据长度超出剩余可读数据范围，无法完成反序列化。");

            ByteBlock block = new(length);
            reader.TryRead(block.GetSpan(length).Slice(0, length));
            block.Advance(length);
            return block;
        }

        public override sealed ByteBlock Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return new();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 ByteBlock 类型，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            ByteBlock block = new(length);
            var readLength = reader.Read(block.GetSpan(length).Slice(0, length));
            block.Advance(readLength);
            return block;
        }

        public override sealed long GetLength(ByteBlock value)
        {
            if (value.IsEmpty)
            {
                return NilLength;
            }
            return _int.GetLength(value.Committed) + 1 + value.Committed;
        }
    }
}