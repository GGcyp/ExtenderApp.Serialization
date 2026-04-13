using ExtenderApp.Buffer;
using ExtenderApp.Serialization;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 版本化数据格式化器管理器，用于管理不同版本的 <see cref="VersionData{T}"/> 序列化/反序列化逻辑
    /// </summary>
    /// <typeparam name="T">需要版本化管理的数据类型</typeparam>
    /// <remarks>
    /// 1. 继承自 <see cref="ResolverFormatter{VersionData{T}}"/>，提供基础的格式化器解析功能
    /// 2. 内部维护格式化器列表，按版本号排序以确保使用正确的格式化器
    /// </remarks>
    internal sealed class VersionDataFormatterManager<T> : ResolverFormatter<VersionData<T>>
    {
        private readonly BinaryFormatter<Version> _version;

        /// <summary>
        /// 已注册的版本化数据格式化器列表，按版本号升序排列。每个格式化器负责特定版本的数据序列化/反序列化逻辑。
        /// </summary>
        public List<BinaryFormatter<T>> Formatters { get; }

        ///<inheritdoc/>
        public override sealed int DefaultLength => Formatters.Last().DefaultLength;

        public VersionDataFormatterManager(BinaryFormatterResolver resolver) : base(resolver)
        {
            Formatters = new();
            _version = resolver.GetFormatter<Version>();
        }

        /// <summary>
        /// 通过指定版本获取对应的格式化器，如果未找到则抛出异常
        /// </summary>
        /// <param name="version">版本号</param>
        /// <returns>对应版本的格式化器</returns>
        /// <exception cref="InvalidOperationException">未找到对应版本的格式化器时抛出</exception>
        public BinaryFormatter<T> GetFormatter(Version version)
        {
            BinaryFormatter<T>? formatter = Formatters.FirstOrDefault(f => VersionPayloadFormatterHelper.GetFormatterVersion(f) == version);
            if (formatter == null)
            {
                throw new InvalidOperationException($"未找到对应版本的格式化器，版本号：{version} ，类型名：{typeof(T).FullName}");
            }
            return formatter;
        }

        /// <summary>
        /// 获取最后一个格式化器，通常用于序列化时使用最新版本的格式化器，如果没有格式化器则抛出异常
        /// </summary>
        /// <returns>最后一个格式化器</returns>
        /// <exception cref="ArgumentOutOfRangeException">未添加任何格式化器时抛出</exception>
        private BinaryFormatter<T> LastFormatter()
        {
            if (Formatters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("还未添加格式化器", typeof(T).FullName);
            }

            return Formatters[Formatters.Count - 1];
        }

        /// <summary>
        /// 添加一个新的格式化器到管理器中，要求为 <see cref="VersionDataFormatter{T}"/> 或 <see cref="AutoVersionDataFormatter{T}"/> 的实例，并且版本号不能重复。添加后会根据版本号自动排序，确保使用正确的格式化器进行序列化/反序列化。
        /// </summary>
        /// <param name="formatter">要添加的格式化器</param>
        /// <exception cref="ArgumentNullException">格式化器为空时抛出</exception>
        /// <exception cref="ArgumentException">格式化器类型不正确时抛出</exception>
        /// <exception cref="InvalidOperationException">格式化器版本重复时抛出</exception>
        public void AddFormatter(object formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter), "格式化器不能为空");
            }
            if (formatter is not BinaryFormatter<T> localFormatter)
            {
                throw new ArgumentException("格式化器必须是 BinaryFormatter<T> 的实例。", nameof(formatter));
            }
            var fv = VersionPayloadFormatterHelper.GetFormatterVersion(localFormatter);
            if (Formatters.Any(f => VersionPayloadFormatterHelper.GetFormatterVersion(f) == fv))
            {
                throw new InvalidOperationException($"不能重复添加相同版本的格式化器：{fv} ：{typeof(T).Name}");
            }

            // 找到第一个版本号比当前大的位置，插入到它前面（保持升序）
            int index = Formatters.FindIndex(f => VersionPayloadFormatterHelper.GetFormatterVersion(f) > fv);
            if (index == -1)
            {
                // 如果没有更大的版本号，直接添加到末尾
                Formatters.Add(localFormatter);
            }
            else
            {
                // 否则插入到该位置，确保大的版本号在最后
                Formatters.Insert(index, localFormatter);
            }
        }

        ///<inheritdoc/>
        public override sealed VersionData<T> Deserialize(ref BinaryReaderAdapter reader)
        {
            var version = _version.Deserialize(ref reader);
            if (version == null)
            {
                return new VersionData<T>(version!, default!);
            }

            var formatter = GetFormatter(version);
            var data = formatter.Deserialize(ref reader);
            return new VersionData<T>(version, data);
        }

        ///<inheritdoc/>
        public override sealed VersionData<T> Deserialize(ref SpanReader<byte> reader)
        {
            var version = _version.Deserialize(ref reader);
            if (version == null)
            {
                return new VersionData<T>(version!, default!);
            }

            var formatter = GetFormatter(version);
            var data = formatter.Deserialize(ref reader);
            return new VersionData<T>(version, data);
        }

        ///<inheritdoc/>
        public T Deserialize(ref BinaryReaderAdapter reader, Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("格式化器版本不能为空", nameof(version));
            }

            var binaryVersion = _version.Deserialize(ref reader);
            if (binaryVersion == null)
            {
                throw new InvalidOperationException($"读取到的版本信息为空，无法确定使用哪个格式化器进行反序列化。序列化类型为：{typeof(T).Name}");
            }

            if (binaryVersion != version)
            {
                throw new InvalidCastException($"文件版本和需要版本不匹配,文件版本：{binaryVersion},需要版本：{version},{typeof(T).FullName}");
            }

            var formatter = GetFormatter(version);
            return formatter.Deserialize(ref reader);
        }

        ///<inheritdoc/>
        public T Deserialize(ref SpanReader<byte> reader, Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("格式化器版本不能为空", nameof(version));
            }

            var binaryVersion = _version.Deserialize(ref reader);
            if (binaryVersion == null)
            {
                throw new InvalidOperationException($"读取到的版本信息为空，无法确定使用哪个格式化器进行反序列化。序列化类型为：{typeof(T).Name}");
            }

            if (binaryVersion != version)
            {
                throw new InvalidCastException($"文件版本和需要版本不匹配,文件版本：{binaryVersion},需要版本：{version},{typeof(T).FullName}");
            }

            var formatter = GetFormatter(version);
            return formatter.Deserialize(ref reader);
        }

        ///<inheritdoc/>
        public override sealed void Serialize(ref BinaryWriterAdapter writer, VersionData<T> value)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            var version = value.DataVersion;
            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }
            _version.Serialize(ref writer, version);

            var formatter = GetFormatter(version);
            formatter.Serialize(ref writer, value.Data);
        }

        ///<inheritdoc/>
        public override sealed void Serialize(ref SpanWriter<byte> writer, VersionData<T> value)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }

            var version = value.DataVersion;
            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }
            _version.Serialize(ref writer, version);

            var formatter = GetFormatter(version);
            formatter.Serialize(ref writer, value.Data);
        }

        ///<inheritdoc/>
        public void Serialize(ref BinaryWriterAdapter writer, T value)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            var formatter = LastFormatter();
            var version = VersionPayloadFormatterHelper.GetFormatterVersion(formatter);
            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }
            _version.Serialize(ref writer, version);
            formatter.Serialize(ref writer, value);
        }

        ///<inheritdoc/>
        public void Serialize(ref SpanWriter<byte> writer, T value)
        {
            BinaryFormatter<T> formatter = LastFormatter();

            _version.Serialize(ref writer, VersionPayloadFormatterHelper.GetFormatterVersion(formatter));
            formatter.Serialize(ref writer, value);
        }

        ///<inheritdoc/>
        public void Serialize(ref BinaryWriterAdapter writer, T value, Version version)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }

            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                WriteNil(ref writer);
                return;
            }

            BinaryFormatter<T> formatter = GetFormatter(version);
            _version.Serialize(ref writer, version);
            formatter.Serialize(ref writer, value);
        }

        ///<inheritdoc/>
        public void Serialize(ref SpanWriter<byte> writer, T value, Version version)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }

            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                WriteNil(ref writer);
                return;
            }

            BinaryFormatter<T> formatter = GetFormatter(version);
            _version.Serialize(ref writer, version);
            formatter.Serialize(ref writer, value);
        }

        ///<inheritdoc/>
        public override sealed long GetLength(VersionData<T> value)
        {
            if (value.IsEmpty)
            {
                return _version.DefaultLength;
            }
            var formatter = GetFormatter(value.DataVersion);
            return _version.GetLength(value.DataVersion) + formatter.GetLength(value.Data);
        }

        ///<inheritdoc/>
        public long GetLength(T value, Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("输入格式化版本不能为空");
            }
            var formatter = GetFormatter(version);
            return _version.GetLength(version) + formatter.GetLength(value);
        }

        ///<inheritdoc/>
        public long GetLength(T value)
        {
            var formatter = LastFormatter();
            return _version.GetLength(VersionPayloadFormatterHelper.GetFormatterVersion(formatter)) + formatter.GetLength(value);
        }
    }
}