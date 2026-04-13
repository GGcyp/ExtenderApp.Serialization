using System.Reflection;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 序列化相关的扩展方法集合。
    /// </summary>
    public static class SerializationExtensions
    {
        #region Reflection Helpers

        /// <summary>
        /// 尝试获取成员上的 <see cref="SerializationsMemberAttribute"/>，如果存在则返回其包含的序列化包含信息。
        /// </summary>
        /// <param name="memberInfo">要检查的成员信息。</param>
        /// <param name="include">如果找到属性，则返回其包含信息；否则返回 <c>false</c>。</param>
        /// <returns>返回是否找到</returns>
        internal static bool TryGetSerializationsMemberAttribute(this MemberInfo memberInfo, out bool include)
        {
            var attribute = memberInfo.GetCustomAttribute<SerializationsMemberAttribute>(true);
            if (attribute is null)
            {
                include = false;
                return false;
            }

            include = attribute.Include;
            return true;
        }

        #endregion Reflection Helpers
    }
}
