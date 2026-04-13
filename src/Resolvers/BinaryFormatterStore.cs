using System.Collections.Concurrent;
using ExtenderApp.Serialization.Contracts;
using ExtenderApp.Serialization.Formatters;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 已注册类型与其二进制格式化器元数据的存储（并发字典）。
    /// </summary>
    public class BinaryFormatterStore : ConcurrentDictionary<Type, BinaryFormatterDetails>
    {
        /// <summary>
        /// 判断类型是否继承 <see cref="VersionDataFormatter{T}" /> 或 <see cref="AutoVersionDataFormatter{T}" />（开放泛型定义匹配）。
        /// </summary>
        internal static bool IsVersionPayloadFormatterType(Type? type)
        {
            for (; type != null; type = type.BaseType)
            {
                if (type.IsGenericType)
                {
                    var def = type.GetGenericTypeDefinition();
                    if (def == typeof(VersionDataFormatter<>) || def == typeof(AutoVersionDataFormatter<>))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 添加格式化器类型到指定类型的元数据中。
        /// </summary>
        /// <param name="type"> 目标类型。 </param>
        /// <param name="typeFormatter"> 格式化器类型。 </param>
        /// <param name="isVersionDataFormatter"> 是否为版本化数据格式化器。 </param>
        public void AddFormatter(Type type, Type typeFormatter, bool isVersionDataFormatter = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (typeFormatter == null)
                throw new ArgumentNullException(nameof(typeFormatter));

            if (TryGetValue(type, out BinaryFormatterDetails? details))
            {
                if (!IsVersionPayloadFormatterType(typeFormatter))
                {
                    throw new InvalidOperationException($"只有版本化载荷格式化器（继承 VersionDataFormatter&lt;&gt; 或 AutoVersionDataFormatter&lt;&gt;）才能重复添加：{type.FullName} : {typeFormatter.FullName}");
                }

                if (details.FormatterTypes.Contains(typeFormatter))
                {
                    throw new Exception($"转换器类型已存在：{type.FullName} : {typeFormatter.FullName}");
                }
                details.FormatterTypes.Add(typeFormatter);
                return;
            }

            details = new BinaryFormatterDetails(type, isVersionDataFormatter);
            details.FormatterTypes.Add(typeFormatter);

            if (isVersionDataFormatter)
            {
                TryAdd(details.BinaryType, details);
                TryAdd(details.VersionDataBinaryType!, details);
            }
            else
            {
                TryAdd(type, details);
            }
        }

        /// <summary>
        /// 尝试获取指定类型的单一格式化器类型。
        /// </summary>
        /// <param name="type"> 目标类型。 </param>
        /// <param name="formatterType"> 输出的格式化器类型。 </param>
        /// <returns> 如果成功获取单一格式化器类型，则返回 true；否则返回 false。 </returns>
        public bool TryGetSingleFormatterType(Type type, out Type formatterType)
        {
            formatterType = default!;
            if (!TryGetValue(type, out var details) ||
                details.IsVersionDataFormatter)
            {
                return false;
            }
            formatterType = details.FormatterTypes[0];
            return true;
        }
    }
}