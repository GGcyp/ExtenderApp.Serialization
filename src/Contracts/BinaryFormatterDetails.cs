namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 表示二进制格式化程序的详细信息，包含格式化程序类型和版本控制信息。
    /// </summary>
    public class BinaryFormatterDetails
    {
        /// <summary>
        /// 获取一个值，指示当前格式化程序是否支持版本控制。
        /// </summary>
        public bool IsVersionDataFormatter { get; }

        /// <summary>
        /// 获取或设置格式化程序类型集合，可以是单个类型或类型列表。
        /// </summary>
        internal ValueOrList<Type> FormatterTypes { get; set; }

        /// <summary>
        /// 需要被序列化类型
        /// </summary>
        public Type BinaryType { get; }

        /// <summary>
        /// 版本数据类型
        /// </summary>
        public Type? VersionDataBinaryType { get; }

        /// <summary>
        /// 初始化 <see cref="BinaryFormatterDetails" /> 类的新实例。
        /// </summary>
        /// <param name="isVersionDataFormatter"> 指示格式化程序是否支持版本控制的标志。 如果为 true，表示格式化程序实现了版本控制接口；否则为 false。 </param>
        public BinaryFormatterDetails(Type binaryType, bool isVersionDataFormatter)
        {
            IsVersionDataFormatter = isVersionDataFormatter;
            FormatterTypes = new ValueOrList<Type>();

            if (binaryType.IsGenericType && binaryType.GetGenericTypeDefinition() == typeof(VersionData<>))
            {
                BinaryType = binaryType.GetGenericArguments()[0];
                VersionDataBinaryType = binaryType;
                IsVersionDataFormatter = true;
            }
            else
            {
                BinaryType = binaryType;
                if (isVersionDataFormatter)
                {
                    VersionDataBinaryType = typeof(VersionData<>).MakeGenericType(BinaryType);
                }
            }
        }
    }
}