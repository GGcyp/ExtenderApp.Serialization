using System.Net;
using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;

namespace ExtenderApp.Serialization.Formatters.Class
{
    /// <summary>
    /// IPEndPoin 二进制格式化器
    /// </summary>
    internal sealed class IPEndPoinFormatter : ResolverFormatter<IPEndPoint>
    {
        private readonly BinaryFormatter<IPAddress> _address;
        private readonly BinaryFormatter<int> _int;

        public IPEndPoinFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _address = resolver.GetFormatter<IPAddress>();
            _int = resolver.GetFormatter<int>();
        }

        public override sealed IPEndPoint Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var address = _address.Deserialize(ref reader);
            var port = _int.Deserialize(ref reader);
            return new IPEndPoint(address, port);
        }

        public override sealed IPEndPoint Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var address = _address.Deserialize(ref reader);
            var port = _int.Deserialize(ref reader);
            return new IPEndPoint(address, port);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, IPEndPoint value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _address.Serialize(ref writer, value.Address);
            _int.Serialize(ref writer, value.Port);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, IPEndPoint value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _address.Serialize(ref writer, value.Address);
            _int.Serialize(ref writer, value.Port);
        }

        public override sealed long GetLength(IPEndPoint value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _int.DefaultLength + _address.GetLength(value.Address);
        }
    }
}