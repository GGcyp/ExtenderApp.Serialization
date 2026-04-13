using System.Linq.Expressions;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 集合辅助类，用于创建指定类型的集合。
    /// </summary>
    /// <typeparam name="TCollection">集合的类型</typeparam>
    public class CollectionHelpers<TCollection> where TCollection : new()
    {
        /// <summary>
        /// 用于创建集合的函数委托。
        /// </summary>
        private Func<int, TCollection>? collectionCreator;

        /// <summary>
        /// 初始化 CollectionHelpers 类的新实例。
        /// </summary>
        public CollectionHelpers()
        {
            collectionCreator = null;
            var ctor = typeof(TCollection).GetConstructors()
                                        .FirstOrDefault(c =>
                                             c.GetParameters().Length == 1 &&
                                             c.GetParameters()[0].ParameterType == typeof(int));
            if (ctor != null)
            {
                ParameterExpression param = Expression.Parameter(typeof(int), "count");
                NewExpression body = Expression.New(ctor, param);
                collectionCreator = Expression.Lambda<Func<int, TCollection>>(body, param).Compile();
            }
        }

        /// <summary>
        /// 创建一个指定大小的集合。
        /// </summary>
        /// <param name="count">集合的大小。</param>
        /// <returns>创建的集合实例。</returns>
        public TCollection CreateCollection(int count) => collectionCreator != null ? collectionCreator.Invoke(count) : new TCollection();
    }
}
