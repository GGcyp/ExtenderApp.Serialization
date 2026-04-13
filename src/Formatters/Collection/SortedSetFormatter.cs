namespace ExtenderApp.Serialization.Formatters.Collection
{
    internal class SortedSetFormatter<T> : InterfaceCollectionFormatter<T, SortedSet<T>>
    {
        public SortedSetFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Add(SortedSet<T> collection, T value)
        {
            collection.Add(value);
        }

        protected override SortedSet<T> Create(int count)
        {
            return new();
        }
    }
}