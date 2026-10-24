namespace OpenApiValidate;

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default!)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
