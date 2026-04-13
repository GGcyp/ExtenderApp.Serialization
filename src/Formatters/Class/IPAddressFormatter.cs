using System.Net;
using ExtenderApp.Buffer;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters.Class
{
    /// <summary>
    /// IPAddress 的二进制格式化器。
    /// </summary>
    public sealed class IPAddressFormatter : BinaryFormatter<IPAddress>
    {
        private const int IPv4Length = 4;
        private const int IPv6Length = 16;

        public override sealed IPAddress Deserialize(ref BinaryReaderAdapter reader)
        {
            IPAddress address = null!;
            if (TryReadNil(ref reader))
            {
                return address;
            }

            if (TryReadMark(ref reader, BinaryOptions.Ex4) && reader.Remaining >= IPv4Length)
            {
                Span<byte> buffer = stackalloc byte[IPv4Length];
                reader.TryRead(buffer);
                address = new IPAddress(buffer);
            }

            if (TryReadMark(ref reader, BinaryOptions.Ex16) && reader.Remaining >= IPv6Length)
            {
                Span<byte> buffer = stackalloc byte[IPv6Length];
                reader.TryRead(buffer);
                address = new IPAddress(buffer);
            }

            return address;
        }

        public override sealed IPAddress Deserialize(ref SpanReader<byte> reader)
        {
            IPAddress address = null!;
            int length = 0;
            if (TryReadNil(ref reader))
            {
                return address;
            }

            if (TryReadMark(ref reader, BinaryOptions.Ex4) && reader.Remaining >= IPv4Length)
            {
                address = new IPAddress(reader.UnreadSpan.Slice(0, IPv4Length));
                length = 4;
            }

            if (TryReadMark(ref reader, BinaryOptions.Ex16) && reader.Remaining >= IPv6Length)
            {
                address = new IPAddress(reader.UnreadSpan.Slice(0, IPv6Length));
                length = IPv6Length;
            }

            reader.Advance(length);
            return address;
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, IPAddress value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            WriteMark(ref writer, GetMark(value));
            value.TryWriteBytes(writer.GetSpan((int)GetLength(value)), out int bytesWritten);
            writer.Advance(bytesWritten);
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, IPAddress value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }
            if (GetLength(value) > writer.Remaining)
            {
                throw new InvalidOperationException("无法写入，写入器剩余空间不足。");
            }

            WriteMark(ref writer, GetMark(value));
            value.TryWriteBytes(writer.UnwrittenSpan, out int bytesWritten);
            writer.Advance(bytesWritten);
        }

        public override sealed long GetLength(IPAddress value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 16;
        }

        private static byte GetMark(IPAddress value)
        {
            return value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? BinaryOptions.Ex4 : BinaryOptions.Ex16;
        }
    }
}