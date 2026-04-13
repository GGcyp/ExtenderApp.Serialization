using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 内存格式化器：提供对 <see cref="Memory{T}"/> 类型的二进制序列化和反序列化支持。 通过将内存视为元素的连续集合，格式化器能够高效地处理内存数据。
    /// 在序列化时，首先写入一个数组头和长度信息，然后逐个序列化内存中的元素；在反序列化时，根据数组头和长度信息重建内存对象。 此格式化器适用于需要在二进制协议中传输内存数据的场景，确保数据结构清晰且易于解析。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class MemoryFormatter<T> : ResolverFormatter<Memory<T>>
    {
        private readonly BinaryFormatter<T> _t;
        private readonly BinaryFormatter<int> _int;

        public MemoryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override sealed Memory<T> Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 Memory<T> 类型的值，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }

            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _t.Deserialize(ref reader);
            }
            return new Memory<T>(array);
        }

        public override sealed Memory<T> Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Memory<T>.Empty;
            }

            if (!TryReadArrayHeader(ref reader))
            {
                ThrowOperationException("无法将当前数据反序列化为 Memory<T> 类型的值，数据格式不匹配。");
            }

            var length = _int.Deserialize(ref reader);
            if (length == 0)
            {
                return Memory<T>.Empty;
            }

            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = _t.Deserialize(ref reader);
            }
            return new Memory<T>(array);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Memory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _t.Serialize(ref writer, value.Span[i]);
            }
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Memory<T> value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                _t.Serialize(ref writer, value.Span[i]);
            }
        }

        public override sealed long GetLength(Memory<T> value)
        {
            if (value.IsEmpty)
                return NilLength;

            long length = _int.GetLength(value.Length); // TargetArray header + _intLength field (approx)
            for (var i = 0; i < value.Length; i++)
            {
                length += _t.GetLength(value.Span[i]);
            }

            return length;
        }
    }
}