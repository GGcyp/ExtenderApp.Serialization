using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 一个抽象类，继承自 <see cref="BinaryFormatter{T}"/>，用于格式化键值对集合。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <typeparam name="TDictionary">具体的字典类型。</typeparam>
    public abstract class InterfaceDictionaryFormatter<TKey, TValue, TDictionary> : ResolverFormatter<TDictionary> where TDictionary : IDictionary<TKey, TValue>
    {
        private readonly BinaryFormatter<TKey> _key;
        private readonly BinaryFormatter<TValue> _value;
        private readonly BinaryFormatter<int> _int;

        protected InterfaceDictionaryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _key = GetFormatter<TKey>();
            _value = GetFormatter<TValue>();
            _int = GetFormatter<int>();
        }

        public override sealed TDictionary Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return default!;
            }

            if (!TryReadMapHeader(ref reader))
            {
                ThrowOperationException("数据不是映射类型。");
            }

            int count = _int.Deserialize(ref reader);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _key.Deserialize(ref reader);
                var value = _value.Deserialize(ref reader);
                dict.Add(key, value);
            }

            return dict;
        }

        public override sealed TDictionary Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return default;
            }

            if (!TryReadMapHeader(ref reader))
            {
                ThrowOperationException("数据不是映射类型。");
            }

            int count = _int.Deserialize(ref reader);
            TDictionary dict = Create(count);
            for (int i = 0; i < count; i++)
            {
                var key = _key.Deserialize(ref reader);
                var value = _value.Deserialize(ref reader);
                dict.Add(key, value);
            }

            return dict;
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, TDictionary value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMapHeader(ref writer);
            _int.Serialize(ref writer, value.Count);
            foreach (var kvp in value)
            {
                _key.Serialize(ref writer, kvp.Key);
                _value.Serialize(ref writer, kvp.Value);
            }
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, TDictionary value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMapHeader(ref writer);
            _int.Serialize(ref writer, value.Count);
            foreach (var kvp in value)
            {
                _key.Serialize(ref writer, kvp.Key);
                _value.Serialize(ref writer, kvp.Value);
            }
        }

        public override sealed long GetLength(TDictionary? value)
        {
            if (value == null)
            {
                return NilLength;
            }

            var result = _int.GetLength(value.Count) + 1;
            foreach (var kvp in value)
            {
                result += _key.GetLength(kvp.Key);
                result += _value.GetLength(kvp.Value);
            }
            return result;
        }

        protected abstract TDictionary Create(int count);
    }
}