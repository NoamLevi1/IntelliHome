using System.Collections;
using System.Collections.Concurrent;

namespace IntelliHome.Common;

public sealed class ConcurrentHashSet<TItem> : IEnumerable<TItem>
    where TItem : notnull
{
    private readonly ConcurrentDictionary<TItem, VoidResult> _concurrentDictionary;

    public ConcurrentHashSet() =>
        _concurrentDictionary = new ConcurrentDictionary<TItem, VoidResult>();

    public IEnumerator<TItem> GetEnumerator() =>
        _concurrentDictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public bool Add(TItem item) =>
        _concurrentDictionary.TryAdd(item, VoidResult.Instance);

    public bool Remove(TItem item) =>
        _concurrentDictionary.TryRemove(item, out _);
}