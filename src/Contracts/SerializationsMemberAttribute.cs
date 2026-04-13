namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 标记字段或属性在序列化中的包含或排除状态。未标注该特性的成员将按默认规则（公共可读写字段/属性）参与序列化。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SerializationsMemberAttribute : Attribute
    {
        /// <summary>
        /// 指示是否将该成员包含在序列化结果中。
        /// </summary>
        public bool Include { get; }

        /// <summary>
        /// 初始化序列化成员标记。
        /// </summary>
        /// <param name="include">为 <c>true</c> 时包含该成员；为 <c>false</c> 时排除。</param>
        public SerializationsMemberAttribute(bool include = true)
        {
            Include = include;
        }
    }
}
