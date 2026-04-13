using ExtenderApp.Buffer;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Serialization.Tests;

/// <summary>
/// 测试用依赖注入与序列化实例构造辅助。
/// </summary>
internal static class SerializationTestHelpers
{
    /// <summary>
    /// 向容器注册与主库内置引导一致的 <see cref="BinaryFormatterResolver" /> 与 <see cref="BinarySerialization" />。
    /// </summary>
    public static IServiceCollection AddSerializations(this IServiceCollection services)
    {
        services.AddSingleton(_ =>
        {
            var store = BinaryFormatterStoreBootstrap.CreateStoreWithBuiltInFormatters();
            return new BinaryFormatterResolver(store);
        });
        services.AddSingleton(sp => new BinarySerialization(sp.GetRequiredService<BinaryFormatterResolver>()));
        return services;
    }

    /// <summary>
    /// 构建已注册序列化服务的 <see cref="ServiceProvider" />。
    /// </summary>
    public static ServiceProvider CreateSerializationServiceProvider() =>
        new ServiceCollection().AddSerializations().BuildServiceProvider();

    /// <summary>
    /// 通过 <see cref="BinarySerialization.Serialize{T}(T)" /> 与基于 <c> SpanReader&lt;byte&gt; </c> 的 <c> Deserialize&lt;T&gt; </c> 做一次往返。
    /// </summary>
    public static T RoundTrip<T>(BinarySerialization serialization, T value)
    {
        var bytes = serialization.Serialize(value);
        var reader = new SpanReader<byte>(bytes);
        var restored = serialization.Deserialize<T>(ref reader);
        Assert.True(reader.IsCompleted);
        return restored;
    }
}