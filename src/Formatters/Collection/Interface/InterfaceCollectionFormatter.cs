using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 一个抽象类，用于将集合类型的数据序列化和反序列化。
    /// </summary>
    /// <typeparam name="T">集合中元素的类型。</typeparam>
    /// <typeparam name="TCollection">集合的类型。</typeparam>
    public abstract class InterfaceCollectionFormatter<T, TCollection> : ResolverFormatter<TCollection>
        where TCollection : ICollection<T>
    {
        /// <summary>
        /// 用于序列化和反序列化集合中单个元素的格式化器。
        /// </summary>
        private readonly BinaryFormatter<T> _t;

        /// <summary>
        /// 用于序列化和反序列化整数的格式化器。
        /// </summary>
        private readonly BinaryFormatter<int> _int;

        protected InterfaceCollectionFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _t = resolver.GetFormatter<T>();
            _int = resolver.GetFormatter<int>();
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, TCollection value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            var count = value.Count;
            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, count);

            foreach (T item in value)
            {
                _t.Serialize(ref writer, item);
            }
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, TCollection value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            var count = value.Count;
            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, count);

            foreach (T item in value)
            {
                _t.Serialize(ref writer, item);
            }
        }

        public override sealed TCollection Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return default!;
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new InvalidOperationException("数据类型不匹配。");
            }

            var len = _int.Deserialize(ref reader);

            var result = Create(len);
            for (int i = 0; i < len; i++)
            {
                Add(result, _t.Deserialize(ref reader));
            }

            return result;
        }

        public override sealed TCollection Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return default!;
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new InvalidOperationException("数据类型不匹配。");
            }

            var len = _int.Deserialize(ref reader);

            var result = Create(len);
            for (int i = 0; i < len; i++)
            {
                Add(result, _t.Deserialize(ref reader));
            }

            return result;
        }

        public override sealed long GetLength(TCollection? value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Count) + 1;
            foreach (var item in value)
            {
                result += _t.GetLength(item);
            }
            return result;
        }

        /// <summary>
        /// 将元素添加到集合中。
        /// </summary>
        /// <param name="collection">目标集合。</param>
        /// <param name="value">要添加的元素。</param>
        protected abstract void Add(TCollection collection, T value);

        /// <summary>
        /// 创建一个指定大小的集合。
        /// </summary>
        /// <param name="count">集合的大小。</param>
        /// <returns>创建的集合对象。</returns>
        protected abstract TCollection Create(int count);
    }
}