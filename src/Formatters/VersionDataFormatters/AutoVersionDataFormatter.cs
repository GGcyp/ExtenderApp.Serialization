using ExtenderApp.Serialization;
using ExtenderApp.Serialization.Formatters.AutoFormatters;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary>
    /// 带版本信息的自动二进制格式化器基类。
    /// 基于 <see cref="AutoFormatter{T}"/> 生成序列化/反序列化委托，并通过 <see cref="FormatterVersion"/> 暴露当前格式版本，
    /// 以支持模型的前后兼容与迁移。
    /// </summary>
    /// <typeparam name="T">要格式化的对象类型。</typeparam>
    public abstract class AutoVersionDataFormatter<T> : AutoFormatter<T>
    {
        /// <summary>
        /// 当前格式化器的格式版本。
        /// 用于区分不同版本的二进制布局，协助读写端进行兼容处理与迁移。
        /// </summary>
        public abstract Version FormatterVersion { get; }


        protected AutoVersionDataFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

    }
}
