namespace IntelliHome.Common;

public static class DictionaryExtension
{
    public static void Aggregate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
    {
        foreach (var (key, value) in other)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}