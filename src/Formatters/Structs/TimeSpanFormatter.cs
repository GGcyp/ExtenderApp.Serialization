using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 时间间隔格式化器
    /// </summary>
    internal class TimeSpanFormatter : ResolverFormatter<TimeSpan>
    {
        private readonly BinaryFormatter<long> _long;
        public override int DefaultLength => _long.DefaultLength;

        public TimeSpanFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _long = GetFormatter<long>();
        }

        public override void Serialize(ref SpanWriter<byte> writer, TimeSpan value)
        {
            _long.Serialize(ref writer, value.Ticks);
        }

        public override void Serialize(ref BinaryWriterAdapter writer, TimeSpan value)
        {
            _long.Serialize(ref writer, value.Ticks);
        }

        public override TimeSpan Deserialize(ref BinaryReaderAdapter reader)
        {
            return new TimeSpan(_long.Deserialize(ref reader));
        }

        public override TimeSpan Deserialize(ref SpanReader<byte> reader)
        {
            return new TimeSpan(_long.Deserialize(ref reader));
        }

        public override long GetLength(TimeSpan value)
        {
            return _long.GetLength(value.Ticks);
        }
    }
}