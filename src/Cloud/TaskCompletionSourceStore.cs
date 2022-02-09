using System.Collections.Concurrent;

namespace IntelliHome.Cloud;

public interface ITaskCompletionSourceStore
{
    void Add(Guid id, TaskCompletionSource<HttpResponseMessage> taskCompletionSource);
    void SetResult(Guid id, HttpResponseMessage httpResponseMessage);
    bool Remove(Guid id);
}

public class TaskCompletionSourceStore : ITaskCompletionSourceStore
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<HttpResponseMessage>> _dictionary;

    public TaskCompletionSourceStore() => _dictionary=new ConcurrentDictionary<Guid, TaskCompletionSource<HttpResponseMessage>>();
    

    public void Add(Guid id, TaskCompletionSource<HttpResponseMessage> taskCompletionSource)
    {
        _dictionary.TryAdd(id, taskCompletionSource);
    }

    public void SetResult(Guid id, HttpResponseMessage httpResponseMessage)
    {
        if (!_dictionary.TryGetValue(id, out var taskCompletionSource))
        {
            throw new Exception($"Failed to get value of [{nameof(id)}={id}]");
        };
        taskCompletionSource.SetResult( httpResponseMessage);
        _dictionary.TryUpdate(id, taskCompletionSource,taskCompletionSource);
    }

    public bool Remove(Guid id) => _dictionary.TryRemove(id,out var result);
}