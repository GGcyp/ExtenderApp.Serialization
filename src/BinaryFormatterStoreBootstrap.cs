using System.Net;
using ExtenderApp.Serialization.Formatters;
using ExtenderApp.Serialization.Formatters.Class;
using ExtenderApp.Serialization.Formatters.Collection;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 构建带内置格式化器注册的 <see cref="BinaryFormatterStore" />。
    /// </summary>
    internal static class BinaryFormatterStoreBootstrap
    {
        /// <summary>
        /// 创建已注册内置标量、字符串与常用集合格式化器的存储实例。
        /// </summary>
        internal static BinaryFormatterStore CreateStoreWithBuiltInFormatters()
        {
            var store = new BinaryFormatterStore();
            RegisterBuiltInFormatters(store);
            return store;
        }

        private static void RegisterBuiltInFormatters(BinaryFormatterStore store)
        {
            store.AddUnManagedFormatter<byte>();
            store.AddUnManagedFormatter<ushort>();
            store.AddUnManagedFormatter<uint>();
            store.AddUnManagedFormatter<ulong>();
            store.AddUnManagedFormatter<sbyte>();
            store.AddUnManagedFormatter<short>();
            store.AddUnManagedFormatter<int>();
            store.AddUnManagedFormatter<long>();

            store.AddUnManagedFormatter<double>();
            store.AddUnManagedFormatter<float>();

            store.AddStructFormatter<Guid, GuidFormatter>();
            store.AddStructFormatter<char, CharFormatter>();
            store.AddStructFormatter<bool, BooleanFormatter>();
            store.AddStructFormatter<DateTime, DateTimeFormatter>();
            store.AddStructFormatter<TimeSpan, TimeSpanFormatter>();

            store.AddClassFormatter<Uri, UriFormatter>();
            store.AddClassFormatter<Type, TypeFormatter>();
            store.AddClassFormatter<string, StringFormatter>();
            store.AddClassFormatter<Version, VersionFoematter>();
            store.AddClassFormatter<IPAddress, IPAddressFormatter>();
            store.AddClassFormatter<IPEndPoint, IPEndPoinFormatter>();
            AddByteArrayFormatter(store);
        }

        private static void AddByteArrayFormatter(BinaryFormatterStore store)
        {
            store.AddNullableFormatter<byte>();
            store.Add<byte[], ByteArrayFormatter>();
            store.Add<Memory<byte>, ByteMemoryFormatter>();
            store.AddListFormatter<byte>();
            store.AddLinkedListFormatter<byte>();
            store.AddQueueFormatter<byte>();
            store.AddStackFormatter<byte>();
        }

        private static void AddUnManagedFormatter<T>(this BinaryFormatterStore store) where T : unmanaged
        {
            store.Add<T, UnManagedFornatter<T>>();
        }
    }
}