namespace ExtenderApp.Serialization.Formatters.Collection
{
    internal class SortedListFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, SortedList<TKey, TValue>> where TKey : notnull
    {
        public SortedListFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override SortedList<TKey, TValue> Create(int count)
        {
            return new(count);
        }
    }
}