using ExtenderApp.Buffer;
using ExtenderApp.Buffer.ValueBuffers;
using ExtenderApp.Serialization.Formatters;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 基于 <see cref="BinaryFormatterResolver" /> 的二进制序列化实现。
    /// </summary>
    public sealed class BinarySerialization
    {
        /// <summary>
        /// 二进制格式化器解析器
        /// </summary>
        private readonly BinaryFormatterResolver _resolver;

        /// <summary>
        /// 使用指定的 <see cref="BinaryFormatterResolver" /> 初始化 <see cref="BinarySerialization" /> 实例。
        /// </summary>
        /// <param name="binaryFormatterResolver"> 用于解析二进制格式化器的解析器。 </param>
        public BinarySerialization(BinaryFormatterResolver binaryFormatterResolver)
        {
            _resolver = binaryFormatterResolver;
        }

        /// <summary>
        /// 使用内置格式化器注册表创建一个默认配置的 <see cref="BinarySerialization" /> 实例（无需依赖注入容器）。
        /// </summary>
        /// <returns> 已配置了解析器并包含内置格式化器的 <see cref="BinarySerialization" /> 实例。 </returns>
        public static BinarySerialization CreateDefault()
        {
            var store = BinaryFormatterStoreBootstrap.CreateStoreWithBuiltInFormatters();
            var resolver = new BinaryFormatterResolver(store);
            return new BinarySerialization(resolver);
        }

        #region Serialize

        /// <summary>
        /// 将指定值以二进制形式序列化到给定的 <see cref="SpanWriter{Byte}" /> 中。
        /// </summary>
        /// <typeparam name="T"> 要序列化的值的类型。 </typeparam>
        /// <param name="writer"> 目标写入器（作为 <see cref="SpanWriter{Byte}" /> 引用传入）。 </param>
        /// <param name="value"> 要序列化的值。 </param>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="writer" /> 的未写入空间为空时抛出。 </exception>
        public void Serialize<T>(ref SpanWriter<byte> writer, T value)
        {
            if (writer.UnwrittenSpan.IsEmpty)
                throw new ArgumentNullException(nameof(writer));

            if (TryGetFormatter(out BinaryFormatter<T> formatter))
            {
                formatter.Serialize(ref writer, value);
            }
        }

        /// <summary>
        /// 将指定值序列化为字节数组。
        /// </summary>
        /// <typeparam name="T"> 要序列化的值的类型。 </typeparam>
        /// <param name="value"> 要序列化的值。 </param>
        /// <returns> 包含序列化结果的字节数组。 </returns>
        public byte[] Serialize<T>(T value)
        {
            Serialize(value, out var buffer);
            var result = buffer.ToArray();
            buffer.TryRelease();
            return result;
        }

        /// <summary>
        /// 将指定值以二进制形式序列化到给定的 <see cref="BinaryWriterAdapter" /> 中。
        /// </summary>
        /// <typeparam name="T"> 要序列化的值的类型。 </typeparam>
        /// <param name="writer"> 目标写入适配器（作为引用传入）。 </param>
        /// <param name="value"> 要序列化的值。 </param>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="writer" /> 为空时抛出。 </exception>
        public void Serialize<T>(ref BinaryWriterAdapter writer, T value)
        {
            if (writer.IsEmpty)
                throw new ArgumentNullException(nameof(writer));

            if (TryGetFormatter(out BinaryFormatter<T> formatter))
            {
                formatter.Serialize(ref writer, value);
            }
        }

        /// <summary>
        /// 将指定值序列化并输出到一个抽象缓冲区中。
        /// </summary>
        /// <typeparam name="T"> 要序列化的值的类型。 </typeparam>
        /// <param name="value"> 要序列化的值。 </param>
        /// <param name="buffer"> 输出的包含序列化数据的缓冲区。 </param>
        public void Serialize<T>(T value, out AbstractBuffer<byte> buffer)
        {
            var sequence = FastSequence<byte>.GetBuffer();
            BinaryWriterAdapter writer = new(sequence);
            Serialize(ref writer, value);
            buffer = sequence.ToBuffer();
            sequence.TryRelease();
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// 从指定的 <see cref="SpanReader{Byte}" /> 中反序列化出类型为 <typeparamref name="T" /> 的值。
        /// </summary>
        /// <typeparam name="T"> 要反序列化的目标类型。 </typeparam>
        /// <param name="reader"> 源读取器（作为引用传入）。 </param>
        /// <returns> 反序列化得到的值，如果没有可用格式化器则返回类型默认值。 </returns>
        public T Deserialize<T>(ref SpanReader<byte> reader)
        {
            if (TryGetFormatter(out BinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(ref reader);
            }
            return default!;
        }

        /// <summary>
        /// 从指定的 <see cref="BinaryReaderAdapter" /> 中反序列化出类型为 <typeparamref name="T" /> 的值。
        /// </summary>
        /// <typeparam name="T"> 要反序列化的目标类型。 </typeparam>
        /// <param name="reader"> 源读取适配器（作为引用传入）。 </param>
        /// <returns> 反序列化得到的值，如果没有可用格式化器则返回默认值（可能为 null）。 </returns>
        public T? Deserialize<T>(ref BinaryReaderAdapter reader)
        {
            if (TryGetFormatter(out BinaryFormatter<T> formatter))
            {
                return formatter.Deserialize(ref reader);
            }
            return default!;
        }

        #endregion Deserialize

        #region Count

        /// <summary>
        /// 获取序列化给定值时所需的字节长度。
        /// </summary>
        /// <typeparam name="T"> 值的类型。 </typeparam>
        /// <param name="value"> 要测量长度的值。 </param>
        /// <returns> 序列化该值所需的字节长度。 </returns>
        public long GetLength<T>(T value)
        {
            return _resolver.GetFormatter<T>().GetLength(value);
        }

        /// <summary>
        /// 获取类型 <typeparamref name="T" /> 的默认序列化长度（如果适用）。
        /// </summary>
        /// <typeparam name="T"> 要查询的类型。 </typeparam>
        /// <returns> 类型的默认序列化长度。 </returns>
        public long GetDefaulLength<T>()
        {
            return _resolver.GetFormatter<T>().DefaultLength;
        }

        /// <summary>
        /// 尝试获取用于类型 <typeparamref name="T" /> 的二进制格式化器。
        /// </summary>
        /// <typeparam name="T"> 要获取格式化器的类型。 </typeparam>
        /// <param name="formatter"> 如果返回 true，则为对应的格式化器实例；否则为 null。 </param>
        /// <returns> 如果成功获取到格式化器则返回 true；否则返回 false。 </returns>
        public bool TryGetFormatter<T>(out BinaryFormatter<T> formatter)
        {
            try
            {
                formatter = _resolver.GetFormatter<T>();
                return formatter != null;
            }
            catch
            {
                formatter = null!;
                return false;
            }
        }

        #endregion Count
    }
}