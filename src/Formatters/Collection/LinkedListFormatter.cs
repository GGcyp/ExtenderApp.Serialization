namespace ExtenderApp.Serialization.Formatters
{
    internal sealed class LinkedListFormatter<T> : InterfaceCollectionFormatter<T, LinkedList<T>>
    {
        public LinkedListFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed void Add(LinkedList<T> collection, T value)
        {
            collection.AddLast(value);
        }

        protected override sealed LinkedList<T> Create(int count)
        {
            return new();
        }
    }
}