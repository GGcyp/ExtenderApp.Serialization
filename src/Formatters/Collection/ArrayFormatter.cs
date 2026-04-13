using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 用于数组格式化的扩展格式化器
    /// </summary>
    /// <typeparam name="T">数组元素的类型</typeparam>
    public sealed class ArrayFormatter<T> : ResolverFormatter<T[]?>
    {
        private readonly BinaryFormatter<T> _t;

        private readonly BinaryFormatter<int> _int;

        /// <summary>
        /// 初始化ArrayFormatter对象
        /// </summary>
        /// <param name="formatter">二进制格式化器</param>
        /// <param name="binarybufferConvert">二进制写入转换器</param>
        /// <param name="binarybufferConvert">二进制读取转换器</param>
        /// <param name="options">二进制选项</param>
        public ArrayFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, T[]? value)
        {
            if (value == null || value == Array.Empty<T>())
            {
                WriteNil(ref writer);
            }
            else
            {
                WriteArrayHeader(ref writer);
                _int.Serialize(ref writer, value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    _t.Serialize(ref writer, value[i]);
                }
            }
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, T[]? value)
        {
            if (value == null || value == Array.Empty<T>())
            {
                WriteNil(ref writer);
            }
            else
            {
                WriteArrayHeader(ref writer);
                _int.Serialize(ref writer, value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    _t.Serialize(ref writer, value[i]);
                }
            }
        }

        public override sealed T[]? Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return Array.Empty<T>();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new InvalidOperationException("数据格式不匹配，无法反序列化为数组");
            }

            var len = _int.Deserialize(ref reader);
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _t.Deserialize(ref reader);
            }
            return array;
        }

        public override sealed T[]? Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Array.Empty<T>();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new InvalidOperationException("数据格式不匹配，无法反序列化为数组");
            }

            var len = _int.Deserialize(ref reader);
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _t.Deserialize(ref reader);
            }
            return array;
        }

        public override sealed long GetLength(T[]? value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Length) + 1;
            for (int i = 0; i < value.Length; i++)
            {
                result += _t.GetLength(value[i]);
            }
            return result;
        }
    }
}