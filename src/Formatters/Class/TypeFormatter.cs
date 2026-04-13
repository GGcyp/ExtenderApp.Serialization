using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 类型格式化器类，继承自 <see cref="ResolverFormatter{T}"/>。
    /// </summary>
    internal sealed class TypeFormatter : ResolverFormatter<Type>
    {
        private readonly BinaryFormatter<string> _string;

        public TypeFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _string = resolver.GetFormatter<string>();
        }

        public override sealed Type Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
                return null!;

            var typeName = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(typeName))
            {
                return null!;
            }
            try
            {
                return Type.GetType(typeName, throwOnError: true)!;
            }
            catch (Exception)
            {
                // 如果类型无法解析，返回 null
                return null!;
            }
        }

        public override sealed Type Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
                return null!;

            var typeName = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(typeName))
            {
                return null!;
            }
            try
            {
                return Type.GetType(typeName, throwOnError: true)!;
            }
            catch (Exception)
            {
                // 如果类型无法解析，返回 null
                return null!;
            }
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Type value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value?.AssemblyQualifiedName ?? string.Empty);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Type value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value?.AssemblyQualifiedName ?? string.Empty);
        }

        public override sealed long GetLength(Type value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _string.GetLength(value.AssemblyQualifiedName);
        }
    }
}