using ExtenderApp.Buffer;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// Boolean 类型的二进制格式化器，将 true 序列化为 1，false 序列化为 0。反序列化时，1 被转换回 true，0 被转换回 false；对于其他值则抛出异常。
    /// </summary>
    internal sealed class BooleanFormatter : BinaryFormatter<Boolean>
    {
        public override sealed void Serialize(ref SpanWriter<byte> writer, bool value)
        {
            writer.Write(value ? BinaryOptions.True : BinaryOptions.False);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, bool value)
        {
            writer.Write(value ? BinaryOptions.True : BinaryOptions.False);
        }

        public override sealed bool Deserialize(ref BinaryReaderAdapter reader)
        {
            if (reader.TryRead(out byte value))
            {
                if (value == BinaryOptions.True)
                {
                    return true;
                }
                else if (value == BinaryOptions.False)
                {
                    return false;
                }
                throw new InvalidOperationException($"无法转换类型: {value}");
            }
            throw new InvalidOperationException("无法读取数据");
        }

        public override sealed bool Deserialize(ref SpanReader<byte> reader)
        {
            byte value = reader.Read();
            if (value == BinaryOptions.True)
            {
                return true;
            }
            else if (value == BinaryOptions.False)
            {
                return false;
            }
            throw new InvalidOperationException($"无法转换类型: {value}");
        }

        public override sealed long GetLength(bool value)
        {
            return NilLength;
        }
    }
}