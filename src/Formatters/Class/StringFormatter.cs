using System.Runtime.CompilerServices;
using System.Text;
using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Buffer;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>继承自 <see cref="ResolverFormatter{T}"/>，用于序列化/反序列化字符串。</remarks>
    internal sealed class StringFormatter : ResolverFormatter<string>
    {
        private const int MinStackallocLength = 4096;
        private const int DefaultInputLength = 5;
        private const int DefaultBufferSize = 32 * 1024;

        private readonly Encoding _encoding;

        public StringFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _encoding = Encoding.GetEncoding(0);
        }

        #region Deserialize

        /// <summary>
        /// 从 <see cref="BinaryReaderAdapter"/> 中反序列化一个字符串。
        /// </summary>
        /// <param name="reader">用于读取字节的 <see cref="BinaryReaderAdapter"/>（按引用传递）。</param>
        /// <returns>反序列化得到的字符串；如果源表示 null/nil，则返回空字符串。</returns>
        public override sealed string Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
                return string.Empty;

            if (!TryGetStringLength(ref reader, out int length))
                ThrowOperationException("无法反序列化为字符串类型，数据标记不匹配。");
            if (length > reader.Remaining)
                throw new InvalidOperationException("数据长度超过剩余数据长度，无法反序列化为字符串类型。");

            string result = string.Empty;
            Span<byte> span = stackalloc byte[MinStackallocLength];
            if (length <= MinStackallocLength)
            {
                reader.TryRead(span.Slice(0, length));
                return _encoding.GetString(span.Slice(0, length));
            }

            var decoder = _encoding.GetDecoder();
            int remaining = length;
            MemoryBlock<char> charMemoryBlock = MemoryBlock<char>.GetBuffer(_encoding.GetMaxCharCount(length));
            while (remaining > 0)
            {
                int read = System.Math.Min(remaining, span.Length);
                reader.TryRead(span.Slice(0, read));
                decoder.Convert(span.Slice(0, read), charMemoryBlock.GetAvailableSpan(), remaining == read, out int bytesUsed, out int charsUsed, out _);
                charMemoryBlock.Advance(charsUsed);
                remaining -= bytesUsed;
            }

            result = new(charMemoryBlock.CommittedSpan);
            charMemoryBlock.TryRelease();
            return result;
        }

        /// <summary>
        /// 从 <see cref="SpanReader{byte}"/> 中反序列化一个字符串。
        /// </summary>
        /// <param name="reader">用于读取字节的 <see cref="SpanReader{byte}"/>（按引用传递）。</param>
        /// <returns>反序列化得到的字符串；如果源表示 null/nil，则返回空字符串。</returns>
        public override sealed string Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
                return string.Empty;

            if (!TryGetStringLength(ref reader, out int length))
                ThrowOperationException("无法反序列化为字符串类型，数据标记不匹配。");

            if (length > reader.Remaining)
                throw new InvalidOperationException("数据长度超过剩余数据长度，无法反序列化为字符串类型。");

            string result = _encoding.GetString(reader.UnreadSpan.Slice(0, length));
            reader.Advance(length);
            return result;
        }

        /// <summary>
        /// 从 <see cref="BinaryReaderAdapter"/> 中尝试读取字符串的长度信息（含 mark 与长度字段解析）。
        /// </summary>
        /// <param name="reader">输入读取器（按引用）。</param>
        /// <param name="length">输出的字符串字节长度（如果解析成功）。</param>
        /// <returns>当数据标记表示字符串并成功解析长度时返回 <c>true</c>，否则会抛出异常或返回 <c>false</c>。</returns>
        private bool TryGetStringLength(ref BinaryReaderAdapter reader, out int length)
        {
            if (reader.Remaining < 2 || !reader.TryRead(out var mark))
                throw new InvalidOperationException("数据长度不足，无法反序列化为字符串类型。");

            var intBufferLength = GetMarkLength(mark);
            Span<byte> intSpan = stackalloc byte[intBufferLength];
            reader.TryRead(intSpan);
            return TryGetStringLength(mark, intSpan, out length);
        }

        /// <summary>
        /// 从 <see cref="SpanReader{byte}"/> 中尝试读取字符串的长度信息（含 mark 与长度字段解析），内联优先。
        /// </summary>
        /// <param name="reader">输入读取器（按引用）。</param>
        /// <param name="length">输出的字符串字节长度（如果解析成功）。</param>
        /// <returns>当数据标记表示字符串并成功解析长度时返回 <c>true</c>，否则会抛出异常或返回 <c>false</c>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetStringLength(ref SpanReader<byte> reader, out int length)
        {
            if (reader.Remaining < 2 || !reader.TryRead(out var mark))
                throw new InvalidOperationException("数据长度不足，无法反序列化为字符串类型。");

            var intBufferLength = GetMarkLength(mark);
            Span<byte> intSpan = stackalloc byte[intBufferLength];
            reader.Read(intSpan);
            return TryGetStringLength(mark, intSpan, out length);
        }

        /// <summary>
        /// 根据给定的字符串类型标记与随后的长度字段字节，解析并返回字符串的字节长度。
        /// </summary>
        /// <param name="mark">表示字符串长度编码形式的标记（如 <see cref="BinaryOptions.Str8"/>）。</param>
        /// <param name="span">包含长度字段字节的只读跨度。</param>
        /// <param name="length">解析得到的字符串字节长度。</param>
        /// <returns>若 <paramref name="mark"/> 表示字符串类型并成功解析长度则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetStringLength(byte mark, ReadOnlySpan<byte> span, out int length)
        {
            length = 4;
            if (mark == BinaryOptions.Str8)
            {
                length = span[0];
                return true;
            }
            else if (mark == BinaryOptions.Str16)
            {
                length = span.Read<ushort>();
                return true;
            }
            else if (mark == BinaryOptions.Str32)
            {
                length = span.Read<int>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据字符串标记返回长度字段占用的字节数（1、2 或 4）。
        /// </summary>
        /// <param name="mark">字符串长度标记字节。</param>
        /// <returns>长度字段的字节数。</returns>
        /// <exception cref="InvalidOperationException">当标记不属于字符串长度标记时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetMarkLength(byte mark)
        {
            if (mark == BinaryOptions.Str8)
            {
                return 1;
            }
            else if (mark == BinaryOptions.Str16)
            {
                return 2;
            }
            else if (mark == BinaryOptions.Str32)
            {
                return 4;
            }
            throw new InvalidOperationException("数据标记不匹配，无法获取字符串长度。");
        }

        #endregion Deserialize

        #region Serialize

        /// <summary>
        /// 将字符串序列化并写入到由 <see cref="BinaryWriterAdapter"/> 包装的 <see cref="IBufferWriter{byte}"/> 中。
        /// </summary>
        /// <param name="writer">目标写入适配器（按引用）。</param>
        /// <param name="value">要序列化的字符串值；若为 <c>null</c> 或空字符串，将写入 Nil 标记。</param>
        public override sealed void Serialize(ref BinaryWriterAdapter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref writer);
                return;
            }

            int maxByteCount = GetBufferSize(value.Length, ref writer, out Span<byte> stringLengthSpan, out byte make);
            int totalLength = 0;

            if (maxByteCount < DefaultBufferSize)
            {
                totalLength = _encoding.GetBytes(value, writer.GetSpan(maxByteCount));
                writer.Advance(totalLength);
            }
            else
            {
                int charIndex = 0;
                int totalCharLength = value.Length;
                var encoder = _encoding.GetEncoder();

                while (charIndex < totalCharLength)
                {
                    int maxBufferSize = _encoding.GetMaxByteCount(totalCharLength - charIndex);
                    maxBufferSize = Math.Min(maxBufferSize, DefaultBufferSize);
                    encoder.Convert(value.AsSpan(charIndex), writer.GetSpan(maxBufferSize), charIndex + maxBufferSize >= totalCharLength, out int charsUsed, out int bytesUsed, out _);
                    charIndex += charsUsed;
                    totalLength += bytesUsed;
                    writer.Advance(bytesUsed);
                }
            }

            WriterStringLength(stringLengthSpan, make, totalLength);
        }

        /// <summary>
        /// 将字符串序列化并写入到给定的 <see cref="SpanWriter{byte}"/>（预先分配的连续缓冲）。
        /// </summary>
        /// <param name="writer">目标写入器（按引用）。</param>
        /// <param name="value">要序列化的字符串；若为 <c>null</c> 或空字符串，将写入 Nil 标记。</param>
        public override sealed void Serialize(ref SpanWriter<byte> writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNil(ref writer);
                return;
            }

            int maxByteCount = GetBufferSize(value.Length, ref writer, out Span<byte> stringLengthSpan, out byte make);
            var span = writer.UnwrittenSpan;
            if (span.Length < maxByteCount)
                throw new InvalidOperationException("SpanWriter剩余空间不足，无法序列化字符串。");

            int byteCount = _encoding.GetBytes(value, span);
            WriterStringLength(stringLengthSpan, make, byteCount);
            writer.Advance(byteCount);
        }

        /// <summary>
        /// 计算所需缓冲区大小并在目标写入器上预写入长度字段的占位空间，返回用于放置字符串字节的缓冲大小。 此方法会在写入器中提前保留长度字段位置并返回一个用于回填的 <see cref="Span{byte}"/>。
        /// </summary>
        /// <param name="characterLength">字符串中字符数。</param>
        /// <param name="writer">目标 <see cref="BinaryWriterAdapter"/>（按引用）。</param>
        /// <param name="output">返回的长度字段占位 <see cref="Span{byte}"/>, 用于后续回填实际长度。</param>
        /// <param name="make">输出的 mark 字节（用于指示长度字段大小）。</param>
        /// <returns>为编码字符串预估的最大字节数（上界）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBufferSize(int characterLength, ref BinaryWriterAdapter writer, out Span<byte> output, out byte make)
        {
            int bufferSize = GetBufferSize(characterLength, writer.GetSpan(DefaultInputLength).Slice(0, DefaultInputLength), out output, out make, out int encodedBytesOffset);
            writer.Advance(encodedBytesOffset);
            return bufferSize;
        }

        /// <summary>
        /// 计算并预留长度字段占位（用于 <see cref="SpanWriter{byte}"/> 路径），并返回估算的最大字节数。
        /// </summary>
        /// <param name="characterLength">字符串中字符数。</param>
        /// <param name="writer">目标 <see cref="SpanWriter{byte}"/>（按引用）。</param>
        /// <param name="output">返回的长度字段占位 <see cref="Span{byte}"/>。</param>
        /// <param name="make">输出的 mark 字节。</param>
        /// <returns>为编码字符串预估的最大字节数（上界）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBufferSize(int characterLength, ref SpanWriter<byte> writer, out Span<byte> output, out byte make)
        {
            if (writer.Remaining < DefaultInputLength)
                throw new InvalidOperationException("SpanWriter剩余空间不足，无法序列化字符串。");

            int bufferSize = GetBufferSize(characterLength, writer.UnwrittenSpan.Slice(0, DefaultInputLength), out output, out make, out int encodedBytesOffset);
            writer.Advance(encodedBytesOffset);
            return bufferSize;
        }

        /// <summary>
        /// 核心缓冲大小计算：根据字符数决定使用的标记类型（Str8/Str16/Str32），并返回估算的最大字节数与长度字段信息。
        /// </summary>
        /// <param name="characterLength">字符串中字符数。</param>
        /// <param name="input">用于写入 mark 与长度字段的输入 span（预留位置）。</param>
        /// <param name="output">返回的用于回写长度字段的 <see cref="Span{byte}"/>。</param>
        /// <param name="make">输出的 mark 字节。</param>
        /// <param name="encodedBytesOffset">长度字段在输入 span 中的偏移量（用于 Advance）。</param>
        /// <returns>为编码字符串预估的最大字节数（上界）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBufferSize(int characterLength, Span<byte> input, out Span<byte> output, out byte make, out int encodedBytesOffset)
        {
            int bufferSize = _encoding.GetMaxByteCount(characterLength);
            encodedBytesOffset = 0;
            make = BinaryOptions.Str32;
            output = Span<byte>.Empty;

            if (characterLength < byte.MaxValue)
            {
                encodedBytesOffset = 2;
                make = BinaryOptions.Str8;
            }
            else if (characterLength < ushort.MaxValue)
            {
                encodedBytesOffset = 3;
                make = BinaryOptions.Str16;
            }
            else
            {
                encodedBytesOffset = 5;
                make = BinaryOptions.Str32;
            }

            output = input.Slice(0, encodedBytesOffset);
            output[0] = make;
            output = output.Slice(1);
            return bufferSize;
        }

        /// <summary>
        /// 将实际字符串字节长度根据 mark 回写到指定的长度字段位置。
        /// </summary>
        /// <param name="span">长度字段占位 <see cref="Span{byte}"/>。</param>
        /// <param name="mark">长度字段的类型标记（Str8/Str16/Str32）。</param>
        /// <param name="length">实际写入的字节长度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriterStringLength(Span<byte> span, byte mark, int length)
        {
            if (mark == BinaryOptions.Str8)
            {
                span.Write((byte)length);
            }
            else if (mark == BinaryOptions.Str16)
            {
                span.Write((short)length);
            }
            else if (mark == BinaryOptions.Str32)
            {
                span.Write(length);
            }
        }

        #endregion Serialize

        public override sealed long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DefaultLength;
            }

            int charLength = value.Length;
            return _encoding.GetMaxByteCount(charLength) + GetStringLengthByteCount(charLength);
        }

        private int GetStringLengthByteCount(int characterLength)
        {
            if (characterLength < byte.MaxValue)
            {
                return 2;
            }
            else if (characterLength < ushort.MaxValue)
            {
                return 3;
            }
            else
            {
                return 5;
            }
        }
    }
}