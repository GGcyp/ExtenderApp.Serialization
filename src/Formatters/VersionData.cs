

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 表示包含版本信息和对应数据的泛型结构体
    /// </summary>
    /// <typeparam name="T">关联数据的类型</typeparam>
    public struct VersionData<T>
    {
        /// <summary>
        /// 获取数据的版本信息
        /// </summary>
        public Version DataVersion { get; }

        /// <summary>
        /// 获取关联的泛型数据
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// 获取一个值，指示当前 VersionData 实例是否为空（未初始化）
        /// </summary>
        /// <value>
        /// 如果 DataVersion 为 null 且 Value 为默认值（default），则返回 true；否则返回 false。
        /// </value>
        /// <remarks>
        /// 判断逻辑：
        /// 1. 检查 DataVersion 是否为 null（版本信息未设置）
        /// 2. 使用泛型类型的默认比较器检查 Value 是否等于默认值
        /// 当且仅当以上两个条件同时满足时，认为实例为空
        /// </remarks>
        public bool IsEmpty => DataVersion == null && EqualityComparer<T>.Default.Equals(Data, default);

        /// <summary>
        /// 初始化 VersionData 结构体的新实例
        /// </summary>
        /// <param name="dataVersion">数据的版本信息</param>
        /// <param name="data">关联的泛型数据</param>
        public VersionData(Version dataVersion, T data)
        {
            DataVersion = dataVersion;
            Data = data;
        }
    }
}
