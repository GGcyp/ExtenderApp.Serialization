using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 版本格式化器：提供将 <see cref="Version"/> 类型序列化为二进制表示和从二进制表示反序列化的功能。 通过序列化版本的主要、次要、构建和修订号来实现。 支持处理 null 值以表示未设置的版本信息。
    /// </summary>
    internal sealed class VersionFoematter : ResolverFormatter<Version>
    {
        private readonly BinaryFormatter<int> _int;

        public VersionFoematter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, Version value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _int.Serialize(ref writer, value.Major);
            _int.Serialize(ref writer, value.Minor);
            _int.Serialize(ref writer, value.Build);
            _int.Serialize(ref writer, value.Revision);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, Version value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _int.Serialize(ref writer, value.Major);
            _int.Serialize(ref writer, value.Minor);
            _int.Serialize(ref writer, value.Build);
            _int.Serialize(ref writer, value.Revision);
        }

        public override sealed Version Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var major = _int.Deserialize(ref reader);
            var minor = _int.Deserialize(ref reader);
            var build = _int.Deserialize(ref reader);
            var revision = _int.Deserialize(ref reader);

            return new Version(major, minor, build, revision);
        }

        public override sealed Version Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var major = _int.Deserialize(ref reader);
            var minor = _int.Deserialize(ref reader);
            var build = _int.Deserialize(ref reader);
            var revision = _int.Deserialize(ref reader);

            return new Version(major, minor, build, revision);
        }

        public override sealed long GetLength(Version value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _int.GetLength(value.Major) +
                   _int.GetLength(value.Minor) +
                   _int.GetLength(value.Build) +
                   _int.GetLength(value.Revision);
        }
    }
}