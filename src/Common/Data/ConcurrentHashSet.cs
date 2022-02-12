using System.Collections;
using System.Collections.Concurrent;

namespace IntelliHome.Common;

public sealed class ConcurrentHashSet<TItem> : IEnumerable<TItem>
    where TItem : notnull
{
    private readonly ConcurrentDictionary<TItem, EmptyStruct> _concurrentDictionary;

    public ConcurrentHashSet() =>
        _concurrentDictionary = new ConcurrentDictionary<TItem, EmptyStruct>();

    public IEnumerator<TItem> GetEnumerator() =>
        _concurrentDictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public bool Add(TItem item) =>
        _concurrentDictionary.TryAdd(item, new EmptyStruct());

    public bool Remove(TItem item) =>
        _concurrentDictionary.TryRemove(item, out _);

    private struct EmptyStruct
    {
    }
}