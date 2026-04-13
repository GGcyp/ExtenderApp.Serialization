using ExtenderApp.Serialization.Formatters.AutoFormatters;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 从版本化载荷格式化器实例读取 <see cref="VersionDataFormatter{T}.FormatterVersion"/> / <see cref="AutoVersionDataFormatter{T}.FormatterVersion"/>。
    /// </summary>
    internal static class VersionPayloadFormatterHelper
    {
        /// <summary>
        /// 获取已注册的版本化子格式化器的协议版本号。
        /// </summary>
        internal static Version GetFormatterVersion<T>(BinaryFormatter<T> formatter)
        {
            if (formatter is VersionDataFormatter<T> v)
                return v.FormatterVersion;
            if (formatter is AutoVersionDataFormatter<T> a)
                return a.FormatterVersion;
            throw new ArgumentException($"类型 {formatter.GetType().FullName} 不是有效的版本化载荷格式化器。", nameof(formatter));
        }
    }
}
