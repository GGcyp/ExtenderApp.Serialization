using ExtenderApp.Serialization.Abstractions;
using ExtenderApp.Serialization.Contracts;

namespace ExtenderApp.Serialization.Formatters
{
    /// <summary> 泛型接口列表格式化器类 </summary> <typeparam name="T">列表中元素的类型</typeparam> <typeparam name="TList">列表的类型，必须实现IList<TLinkClient>接口并且有一个无参构造函数</typeparam>
    public class InterfaceListFormatter<T, TList> : InterfaceCollectionFormatter<T, TList> where TList : class, IList<T>, new()
    {
        private readonly CollectionHelpers<TList> _helpers;

        public InterfaceListFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            _helpers = new CollectionHelpers<TList>();
        }

        protected override sealed void Add(TList collection, T value)
        {
            collection.Add(value);
        }

        protected override sealed TList Create(int count)
        {
            return _helpers.CreateCollection(count);
        }
    }
}