namespace ExtenderApp.Serialization.Formatters.Collection
{
    internal class SortedDictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, SortedDictionary<TKey, TValue>> where TKey : notnull
    {
        public SortedDictionaryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override SortedDictionary<TKey, TValue> Create(int count)
        {
            return new();
        }
    }
}