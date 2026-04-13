using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using ExtenderApp.Serialization.Formatters;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// <see cref="BinaryFormatterResolver"/> 的扩展方法。
    /// </summary>
    internal static class BinaryFormatterResolverExtensions
    {
        /// <summary>
        /// 使用给定的解析器获取并验证指定类型的二进制格式化器。
        /// </summary>
        /// <typeparam name="T">要格式化的类型。</typeparam>
        /// <param name="resolver">解析器实例。</param>
        /// <returns>指定类型的 <see cref="BinaryFormatter{T}"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryFormatter<T> GetFormatterWithVerify<T>(this BinaryFormatterResolver resolver)
        {
            if (resolver is null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            BinaryFormatter<T>? formatter;
            try
            {
                formatter = resolver.GetFormatter<T>();
            }
            catch (TypeInitializationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                throw null!;
            }

            if (formatter == null)
            {
                throw new Exception(string.Format("在转换二进制时发现未注册的类型：{0}", typeof(T).FullName));
            }

            return formatter;
        }
    }
}
