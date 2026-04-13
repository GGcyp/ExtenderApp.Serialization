using System.Runtime.CompilerServices;
using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    internal sealed class EnumFormatter<T> : ResolverFormatter<T>
        where T : struct, Enum
    {
        private delegate void EnumSpanWriterSerialize(ref SpanWriter<byte> writer, T value);

        private delegate T EnumBinaryReaderAdapterDeserialize(ref BinaryReaderAdapter reader);

        private delegate T EnumSpanReaderDeserialize(ref SpanReader<byte> reader);

        private delegate long EnumGetLength(T value);

        // 存储非泛型委托与状态；针对 BinaryWriterAdapter 的泛型序列化路径在运行时根据底层类型选择对应的格式化器调用。
        private EnumSpanWriterSerialize spanWriterSerializer = default!;

        private EnumBinaryReaderAdapterDeserialize abstractBufferReaderDeserialize = default!;
        private EnumSpanReaderDeserialize spanReaderDeserialize = default!;
        private EnumGetLength getLength = default!;

        public override sealed int DefaultLength { get; }

        public EnumFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            var type = typeof(T).GetEnumUnderlyingType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    DefaultLength = CreateEnumSerialize(GetFormatter<byte>());
                    break;

                case TypeCode.Int16:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int16>());
                    break;

                case TypeCode.Int32:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int32>());
                    break;

                case TypeCode.Int64:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int64>());
                    break;

                case TypeCode.SByte:
                    DefaultLength = CreateEnumSerialize(GetFormatter<SByte>());
                    break;

                case TypeCode.UInt16:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt16>());
                    break;

                case TypeCode.UInt32:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt32>());
                    break;

                case TypeCode.UInt64:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt64>());
                    break;

                default:
                    throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
            }
        }

        private int CreateEnumSerialize<TType>(BinaryFormatter<TType> formatter)
            where TType : struct
        {
            // 保存非泛型委托（针对 SpanWriter/SpanReader/AbstractBufferReaderAdapter 路径）。
            spanWriterSerializer = (ref SpanWriter<byte> writer, T value) => formatter.Serialize(ref writer, Unsafe.As<T, TType>(ref value));
            abstractBufferReaderDeserialize = (ref BinaryReaderAdapter reader) => { var v = formatter.Deserialize(ref reader); return Unsafe.As<TType, T>(ref v); };
            spanReaderDeserialize = (ref SpanReader<byte> reader) => { var v = formatter.Deserialize(ref reader); return Unsafe.As<TType, T>(ref v); };
            getLength = (T val) => formatter.GetLength(Unsafe.As<T, TType>(ref val));

            return formatter.DefaultLength;
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, T value)
        {
            spanWriterSerializer.Invoke(ref writer, value);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, T value)
        {
            Span<byte> span = writer.GetSpan(DefaultLength);
            SpanWriter<byte> spanWriter = new SpanWriter<byte>(span);
            spanWriterSerializer.Invoke(ref spanWriter, value);
            writer.Advance(DefaultLength);
        }

        public override sealed T Deserialize(ref BinaryReaderAdapter reader)
        {
            return abstractBufferReaderDeserialize.Invoke(ref reader);
        }

        public override sealed T Deserialize(ref SpanReader<byte> reader)
        {
            return spanReaderDeserialize.Invoke(ref reader);
        }

        public override sealed long GetLength(T value)
        {
            return getLength.Invoke(value);
        }
    }
}