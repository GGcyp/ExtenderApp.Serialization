using ExtenderApp.Serialization;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Tests;

/// <summary>
/// 自定义模型序列化行为测试，验证成员包含/排除特性是否生效。
/// </summary>
public sealed class CustomModelSerializationTests
{
    /// <summary>
    /// 自定义类中标记为排除的属性在往返后应恢复默认值。
    /// </summary>
    [Fact]
    public void RoundTrip_CustomClass_ExcludedPropertyIsNotSerialized()
    {
        var serialization = CreateSerialization();
        var value = new CustomUser
        {
            Id = 100,
            Name = "alice",
            Secret = "token-123"
        };

        var back = SerializationTestHelpers.RoundTrip(serialization, value);

        Assert.Equal(100, back.Id);
        Assert.Equal("alice", back.Name);
        Assert.Null(back.Secret);
    }

    /// <summary>
    /// 自定义结构体中标记为排除的属性在往返后应恢复默认值。
    /// </summary>
    [Fact]
    public void RoundTrip_CustomStruct_ExcludedPropertyIsNotSerialized()
    {
        var serialization = CreateSerialization();
        var value = new CustomPoint
        {
            X = 3,
            Y = 4,
            Hidden = 99
        };

        var back = SerializationTestHelpers.RoundTrip(serialization, value);

        Assert.Equal(3, back.X);
        Assert.Equal(4, back.Y);
        Assert.Equal(0, back.Hidden);
    }

    /// <summary>
    /// 构造独立序列化实例，避免依赖容器生命周期。
    /// </summary>
    private static BinarySerialization CreateSerialization()
    {
        var store = BinaryFormatterStoreBootstrap.CreateStoreWithBuiltInFormatters();
        var resolver = new BinaryFormatterResolver(store);
        return new BinarySerialization(resolver);
    }

    /// <summary>
    /// 用于验证属性排除行为的自定义类。
    /// </summary>
    private sealed class CustomUser
    {
        /// <summary>
        /// 用户标识。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名。
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 不应被序列化的敏感字段。
        /// </summary>
        [SerializationsMember(false)]
        public string? Secret { get; set; }
    }

    /// <summary>
    /// 用于验证结构体属性排除行为的自定义结构体。
    /// </summary>
    private struct CustomPoint
    {
        /// <summary>
        /// X 坐标。
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y 坐标。
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 不应被序列化的隐藏值。
        /// </summary>
        [SerializationsMember(false)]
        public int Hidden { get; set; }
    }
}