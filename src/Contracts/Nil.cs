using System.Diagnostics.CodeAnalysis;

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 表示一个可空或无效状态的轻量级结构体 实现了与bool的隐式转换，可用于简化空值判断逻辑
    /// </summary>
    public struct Nil : IEquatable<Nil>
    {
        /// <summary>
        /// 标识当前是否为Nil状态
        /// </summary>
        public bool IsNil;

        /// <summary>
        /// 初始化Nil结构体
        /// </summary>
        /// <param name="isNil"> 是否为Nil状态 </param>
        public Nil(bool isNil)
        {
            IsNil = isNil;
        }

        /// <summary>
        /// 比较两个Nil结构体是否相等
        /// </summary>
        /// <param name="other"> 要比较的另一个Nil结构体 </param>
        /// <returns> 返回比较结果 </returns>
        public bool Equals(Nil other)
        {
            return other.IsNil == IsNil;
        }

        /// <summary>
        /// 重载==运算符，比较两个Nil结构体
        /// </summary>
        public static bool operator ==(Nil left, Nil right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 重载!=运算符，比较两个Nil结构体
        /// </summary>
        public static bool operator !=(Nil left, Nil right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 隐式转换为bool类型 允许直接在条件语句中使用Nil对象
        /// </summary>
        public static implicit operator bool(Nil nil)
        {
            return nil.IsNil;
        }

        /// <summary>
        /// 从bool隐式转换为Nil类型 允许直接使用bool值创建Nil对象
        /// </summary>
        public static implicit operator Nil(bool nilBool)
        {
            return new Nil(nilBool);
        }

        /// <summary>
        /// 重写Equals方法，支持与object类型的比较
        /// </summary>
        public override bool Equals([NotNull] object obj)
        {
            return obj is Nil && Equals((Nil)obj);
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hc = new HashCode();
            hc.Add(IsNil);
            return hc.ToHashCode();
        }

        /// <summary>
        /// 返回当前Nil状态的字符串表示
        /// </summary>
        public override string ToString()
        {
            return IsNil.ToString();
        }
    }
}