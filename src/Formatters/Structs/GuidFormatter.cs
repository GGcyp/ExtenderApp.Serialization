using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// Guid格式化器
    /// </summary>
    internal sealed class GuidFormatter : BinaryFormatter<Guid>
    {
        private const int GuidByteLength = 16;

        public override sealed int DefaultLength => GuidByteLength;

        public override sealed void Serialize(ref SpanWriter<byte> writer, Guid value)
        {
            value.TryWriteBytes(writer.UnwrittenSpan.Slice(0, GuidByteLength));
            writer.Advance(GuidByteLength);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Guid value)
        {
            value.TryWriteBytes(writer.GetSpan(GuidByteLength).Slice(0, GuidByteLength));
            writer.Advance(GuidByteLength);
        }

        public override sealed Guid Deserialize(ref BinaryReaderAdapter reader)
        {
            Span<byte> span = stackalloc byte[GuidByteLength];
            reader.TryRead(span);
            return new Guid(span);
        }

        public override sealed Guid Deserialize(ref SpanReader<byte> reader)
        {
            Span<byte> span = stackalloc byte[GuidByteLength];
            reader.Read(span);
            return new Guid(span);
        }

        public override sealed long GetLength(Guid value)
        {
            return GuidByteLength;
        }
    }
}