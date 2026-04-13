using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Serialization.Tests;

/// <summary>
/// 依赖注入注册与解析器解析的烟雾测试。
/// </summary>
public sealed class SerializationSmokeTests
{
    /// <summary>
    /// 注册 <c> AddSerializations </c> 后，应能从容器解析出非空的 <see cref="BinarySerialization" /> 与 <see cref="BinaryFormatterResolver" />。
    /// </summary>
    [Fact]
    public void AddSerializations_ResolvesBinarySerializationAndResolver()
    {
        using var provider = SerializationTestHelpers.CreateSerializationServiceProvider();

        var serialization = provider.GetRequiredService<BinarySerialization>();
        var resolver = provider.GetRequiredService<BinaryFormatterResolver>();

        Assert.NotNull(serialization);
        Assert.NotNull(resolver);
    }

    /// <summary>
    /// 对同一类型多次调用 <see cref="BinaryFormatterResolver.GetFormatter{T}" /> 应得到非空且同一实例的格式化器。
    /// </summary>
    [Fact]
    public void GetFormatter_ForInt_ReturnsSameNonNullInstance()
    {
        using var provider = SerializationTestHelpers.CreateSerializationServiceProvider();
        var resolver = provider.GetRequiredService<BinaryFormatterResolver>();

        var a = resolver.GetFormatter<int>();
        var b = resolver.GetFormatter<int>();

        Assert.NotNull(a);
        Assert.Same(a, b);
    }
}