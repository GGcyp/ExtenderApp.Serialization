using System.Runtime.InteropServices;
using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    internal sealed class ReadOnlyMemoryFormatter<T> : ResolverFormatter<ReadOnlyMemory<T>>
    {
        private readonly BinaryFormatter<Memory<T>> _memory;

        public ReadOnlyMemoryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _memory = GetFormatter<Memory<T>>();
        }

        public override sealed ReadOnlyMemory<T> Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }

            return _memory.Deserialize(ref reader);
        }

        public override sealed ReadOnlyMemory<T> Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }

            return _memory.Deserialize(ref reader);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            _memory.Serialize(ref writer, MemoryMarshal.AsMemory(value));
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            _memory.Serialize(ref writer, MemoryMarshal.AsMemory(value));
        }

        public override sealed long GetLength(ReadOnlyMemory<T> value)
        {
            if (value.IsEmpty)
                return NilLength;

            return _memory.GetLength(MemoryMarshal.AsMemory(value));
        }
    }
}