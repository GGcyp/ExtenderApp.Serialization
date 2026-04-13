using ExtenderApp.Serialization.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Serialization.Tests;

/// <summary>
/// 二进制序列化往返测试（标量、集合与内部契约类型）。
/// </summary>
public sealed class SerializationRoundTripTests
{
    /// <summary>
    /// 构造与内置引导一致的序列化实例；避免在单测中释放 <see cref="ServiceProvider" /> 后仍使用其解析出的单例，故不用容器解析出的单例做往返。
    /// </summary>
    private static BinarySerialization CreateStandaloneSerialization()
    {
        var store = BinaryFormatterStoreBootstrap.CreateStoreWithBuiltInFormatters();
        var resolver = new BinaryFormatterResolver(store);
        return new BinarySerialization(resolver);
    }

    /// <summary>
    /// <see cref="int" /> 标量序列化再反序列化后应与原值相等。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void RoundTrip_Int_PreservesValue(int value)
    {
        var s = CreateStandaloneSerialization();
        Assert.Equal(value, SerializationTestHelpers.RoundTrip(s, value));
    }

    /// <summary>
    /// <see cref="long" /> 标量往返后应与原值相等。
    /// </summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(1L << 40)]
    public void RoundTrip_Long_PreservesValue(long value)
    {
        var s = CreateStandaloneSerialization();
        Assert.Equal(value, SerializationTestHelpers.RoundTrip(s, value));
    }

    /// <summary>
    /// <see cref="bool" /> 标量往返后应与原值相等。
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTrip_Bool_PreservesValue(bool value)
    {
        var s = CreateStandaloneSerialization();
        Assert.Equal(value, SerializationTestHelpers.RoundTrip(s, value));
    }

    /// <summary>
    /// <see cref="string" />（含空串与 Unicode）往返后应与原值相等。
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("中文")]
    public void RoundTrip_String_PreservesValue(string value)
    {
        var s = CreateStandaloneSerialization();
        Assert.Equal(value, SerializationTestHelpers.RoundTrip(s, value));
    }

    /// <summary>
    /// <see cref="List{T}" />（元素为 <see cref="int" />）往返后序列内容应一致。
    /// </summary>
    [Fact]
    public void RoundTrip_ListOfInt_PreservesElements()
    {
        var s = CreateStandaloneSerialization();
        var value = new List<int> { 1, 2, 3 };
        var back = SerializationTestHelpers.RoundTrip(s, value);
        Assert.Equal(value, back);
    }

    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}" />（ <c> string </c>→ <c> int </c>）往返后键值对应关系应保持。
    /// </summary>
    [Fact]
    public void RoundTrip_DictionaryStringInt_PreservesEntries()
    {
        var s = CreateStandaloneSerialization();
        var value = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var back = SerializationTestHelpers.RoundTrip(s, value);
        Assert.Equal(value.Count, back.Count);
        Assert.Equal(1, back["a"]);
        Assert.Equal(2, back["b"]);
    }

    /// <summary>
    /// 内部类型 <see cref="ValueOrList{T}" /> 在仅含单个元素时往返后应仍为单元素且值正确。
    /// </summary>
    [Fact]
    public void RoundTrip_ValueOrListInt_SingleElement()
    {
        var s = CreateStandaloneSerialization();
        var value = new ValueOrList<int>();
        value.Add(7);
        var back = SerializationTestHelpers.RoundTrip(s, value);
        var single = Assert.Single(back);
        Assert.Equal(7, single);
    }

    /// <summary>
    /// <see cref="ValueOrList{T}" /> 在含多个元素时往返后顺序与取值应保持。
    /// </summary>
    [Fact]
    public void RoundTrip_ValueOrListInt_MultipleElements()
    {
        var s = CreateStandaloneSerialization();
        var value = new ValueOrList<int>();
        value.Add(1);
        value.Add(2);
        var back = SerializationTestHelpers.RoundTrip(s, value);
        Assert.Equal(2, back.Count);
        Assert.Equal(1, back[0]);
        Assert.Equal(2, back[1]);
    }
}