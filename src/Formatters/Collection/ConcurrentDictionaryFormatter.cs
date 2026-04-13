using System.Collections.Concurrent;
using ExtenderApp.Serialization.Abstractions;

namespace ExtenderApp.Serialization.Formatters.Collection
{
    internal sealed class ConcurrentDictionaryFormatter<TKey, TValue> : InterfaceDictionaryFormatter<TKey, TValue, ConcurrentDictionary<TKey, TValue>> where TKey : notnull
    {
        public ConcurrentDictionaryFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override sealed ConcurrentDictionary<TKey, TValue> Create(int count)
        {
            return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, count);
        }
    }
}