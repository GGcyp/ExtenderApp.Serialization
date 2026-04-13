using ExtenderApp.Serialization.Formatters;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// BinaryFormatterResolverStore 类的扩展方法。
    /// </summary>
    internal static class BinaryFormatterStoreExtensions
    {
        /// <summary>
        /// 向BinaryFormatterResolverStore添加指定类型的格式化器
        /// </summary>
        /// <typeparam name="Type"> 要序列化的类型，必须为类类型 </typeparam>
        /// <typeparam name="TFormatter"> 用于序列化和反序列化Type类型的格式化器，必须实现BinaryFormatter接口 </typeparam>
        /// <param name="store"> BinaryFormatterResolverStore实例 </param>
        /// <returns> 返回更新后的BinaryFormatterResolverStore实例 </returns>
        public static BinaryFormatterStore AddClassFormatter<Type, TFormatter>(this BinaryFormatterStore store) where Type : class where TFormatter : BinaryFormatter<Type>
        {
            store.Add<Type, TFormatter>();
            //_store.AddCollectionFormatter<StartupType>();
            return store;
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterStore" /> 添加一个针对结构体类型的格式化器。
        /// </summary>
        /// <typeparam name="Type"> 结构体类型。 </typeparam>
        /// <typeparam name="TFormatter"> 实现 <see cref="BinaryFormatter{T}" /> 接口的格式化器类型。 </typeparam>
        /// <param name="store"> 要添加格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddStructFormatter<Type, TFormatter>(this BinaryFormatterStore store) where Type : struct where TFormatter : BinaryFormatter<Type>
        {
            store.Add<Type, TFormatter>();
            store.AddNullableFormatter<Type>();
            //_store.AddCollectionFormatter<StartupType>();
            return store;
        }

        /// <summary>
        /// 为指定类型的集合添加格式化器。
        /// </summary>
        /// <typeparam name="Type"> 集合元素的类型。 </typeparam>
        /// <param name="store"> BinaryFormatterResolverStore 实例。 </param>
        /// <returns> 返回添加了格式化器的 BinaryFormatterResolverStore 实例。 </returns>
        public static BinaryFormatterStore AddCollectionFormatter<Type>(this BinaryFormatterStore store)
        {
            store.AddArrayFormatter<Type>();
            store.AddListFormatter<Type>();
            store.AddLinkedListFormatter<Type>();
            store.AddQueueFormatter<Type>();
            store.AddStackFormatter<Type>();
            return store;
        }

        /// <summary>
        /// 向 BinaryFormatterResolverStore 中添加一个可空类型的格式化器。
        /// </summary>
        /// <typeparam name="Type"> 要添加格式化器的类型，该类型必须为值类型。 </typeparam>
        /// <param name="store"> 当前的 BinaryFormatterResolverStore 实例。 </param>
        /// <returns> 返回当前的 BinaryFormatterResolverStore 实例。 </returns>
        public static BinaryFormatterStore AddNullableFormatter<Type>(this BinaryFormatterStore store) where Type : struct
        {
            return store.Add<Type?, NullableFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterStore" /> 添加数组类型的格式化器。
        /// </summary>
        /// <typeparam name="Type"> 数组元素的类型。 </typeparam>
        /// <param name="store"> 需要添加格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddArrayFormatter<Type>(this BinaryFormatterStore store)
        {
            return store.Add<Type[], ArrayFormatter<Type>>();
        }

        /// <summary>
        /// 向二进制格式化器解析器存储中添加枚举格式化器。
        /// </summary>
        /// <typeparam name="Type"> 枚举类型。 </typeparam>
        /// <param name="store"> 二进制格式化器解析器存储实例。 </param>
        /// <returns> 添加枚举格式化器后的二进制格式化器解析器存储实例。 </returns>
        public static BinaryFormatterStore AddEnumFormatter<Type>(this BinaryFormatterStore store) where Type : struct, Enum
        {
            return store.Add<Type, EnumFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterStore" /> 添加列表类型的格式化器。
        /// </summary>
        /// <typeparam name="Type"> 列表元素的类型。 </typeparam>
        /// <param name="store"> 需要添加格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddListFormatter<Type>(this BinaryFormatterStore store)
        {
            return store.Add<List<Type>, ListFormatter<Type>>();
        }

        /// <summary>
        /// 为指定的 <see cref="BinaryFormatterStore" /> 添加 <see cref="StackFormatter{T}" /> 格式化器。
        /// </summary>
        /// <typeparam name="Type"> 泛型类型。 </typeparam>
        /// <param name="store"> <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回修改后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="store" /> 为 <c> null </c>。 </exception>
        public static BinaryFormatterStore AddStackFormatter<Type>(this BinaryFormatterStore store)
        {
            return store.Add<Stack<Type>, StackFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterStore" /> 添加 <see cref="LinkedListFormatter{T}" /> 格式化器。
        /// </summary>
        /// <typeparam name="Type"> 链表中存储的元素类型。 </typeparam>
        /// <param name="store"> 当前的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddLinkedListFormatter<Type>(this BinaryFormatterStore store)
        {
            return store.Add<LinkedList<Type>, LinkedListFormatter<Type>>();
        }

        /// <summary>
        /// 为指定的 <see cref="BinaryFormatterStore" /> 添加一个队列格式化器。
        /// </summary>
        /// <typeparam name="Type"> 队列中元素的类型。 </typeparam>
        /// <param name="store"> 要添加队列格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加队列格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddQueueFormatter<Type>(this BinaryFormatterStore store)
        {
            return store.Add<Queue<Type>, QueueFormatter<Type>>();
        }

        /// <summary>
        /// 向给定的 BinaryFormatterResolverStore 添加 DictionaryFormatter。
        /// </summary>
        /// <typeparam name="TKey"> 字典键的类型。 </typeparam>
        /// <typeparam name="TValue"> 字典值的类型。 </typeparam>
        /// <param name="store"> 要添加格式化器的 BinaryFormatterResolverStore。 </param>
        /// <returns> 添加了 DictionaryFormatter 的 BinaryFormatterResolverStore。 </returns>
        public static BinaryFormatterStore AddDictionaryFormatter<TKey, TValue>(this BinaryFormatterStore store) where TKey : notnull
        {
            return store.Add<Dictionary<TKey, TValue>, DictionaryFormatter<TKey, TValue>>();
        }

        /// <summary>
        /// 向 <see cref="BinaryFormatterStore" /> 添加一个自定义的字典格式化器。
        /// </summary>
        /// <typeparam name="TKey"> 字典键的类型。 </typeparam>
        /// <typeparam name="TValue"> 字典值的类型。 </typeparam>
        /// <typeparam name="TDictionary"> 字典的类型，需要实现 <see cref="IDictionary{TKey, TValue}" /> 接口。 </typeparam>
        /// <param name="store"> 要添加格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddInterfaceDictionaryFormatter<TKey, TValue, TDictionary>(this BinaryFormatterStore store) where TDictionary : IDictionary<TKey, TValue>, new()
        {
            return store.Add<TDictionary, CustomizeDictionaryFormatter<TKey, TValue, TDictionary>>();
        }

        /// <summary>
        /// 为 <see cref="BinaryFormatterStore" /> 添加接口列表格式化器。
        /// </summary>
        /// <typeparam name="T"> 接口列表中的元素类型。 </typeparam>
        /// <typeparam name="TList"> 实现了 <see cref="IList{T}" /> 接口的列表类型。 </typeparam>
        /// <param name="store"> 当前 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加了接口列表格式化器的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore AddInterfaceListFormatter<T, TList>(this BinaryFormatterStore store) where TList : class, IList<T>, new()
        {
            return store.Add<TList, InterfaceListFormatter<T, TList>>();
        }

        /// <summary>
        /// 为 <see cref="BinaryFormatterStore" /> 添加指定类型的格式化器。
        /// </summary>
        /// <typeparam name="Type"> 需要添加格式化器的类型。 </typeparam>
        /// <typeparam name="TFormatter"> 继承 <see cref="BinaryFormatter{Type}" /> 的格式化器类型。 </typeparam>
        /// <param name="store"> 当前的 <see cref="BinaryFormatterStore" /> 实例。 </param>
        /// <returns> 返回添加格式化器后的 <see cref="BinaryFormatterStore" /> 实例。 </returns>
        public static BinaryFormatterStore Add<Type, TFormatter>(this BinaryFormatterStore store) where TFormatter : BinaryFormatter<Type>
        {
            store.AddFormatter(typeof(Type), typeof(TFormatter));
            return store;
        }

        /// <summary>
        /// 为指定类型添加版本化数据格式化器，用于处理 <see cref="VersionData{T}" /> 类型的序列化/反序列化
        /// </summary>
        /// <typeparam name="Type"> 需要版本化管理的原始数据类型 </typeparam>
        /// <typeparam name="TFormatter">
        /// 版本化数据格式化器类型，须为 <see cref="VersionDataFormatter{Type}" /> 或 <see cref="AutoVersionDataFormatter{Type}" /> 的派生类。
        /// </typeparam>
        public static BinaryFormatterStore AddVersionData<Type, TFormatter>(this BinaryFormatterStore store)
            where TFormatter : BinaryFormatter<Type>
        {
            store.AddFormatter(typeof(Type), typeof(TFormatter), true);
            return store;
        }
    }
}