using System.Collections.Concurrent;
using IntelliHome.Common;

namespace IntelliHome.Cloud;

public interface IHomeApplianceTunneledHttpMessageHandler
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
    void ReceiveHttpResponse(ReceiveHttpResponseRequest receiveHttpResponseRequest);
}

public sealed class HomeApplianceTunneledHttpMessageHandler : IHomeApplianceTunneledHttpMessageHandler
{
    private readonly ILogger<HomeApplianceTunneledHttpMessageHandler> _logger;
    private readonly IHomeApplianceHttpRequestMessageSenderHub _homeApplianceHttpRequestMessageSenderHub;

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<HttpResponseMessage>> _idToTaskCompletionSourceMapping;

    public HomeApplianceTunneledHttpMessageHandler(
        ILogger<HomeApplianceTunneledHttpMessageHandler> logger,
        IHomeApplianceHttpRequestMessageSenderHub homeApplianceHttpRequestMessageSenderHub)
    {
        _logger = logger;
        _homeApplianceHttpRequestMessageSenderHub = homeApplianceHttpRequestMessageSenderHub;

        _idToTaskCompletionSourceMapping = new ConcurrentDictionary<Guid, TaskCompletionSource<HttpResponseMessage>>();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();

        if (!_idToTaskCompletionSourceMapping.TryAdd(id, taskCompletionSource))
        {
            throw new Exception($"Id already exists [{nameof(id)}={id}]");
        }

        try
        {
            await _homeApplianceHttpRequestMessageSenderHub.SendRequestAsync(new SendHttpRequestRequest(id, httpRequestMessage), cancellationToken);

            return await taskCompletionSource.Task.WithTimeoutAsync(TimeSpan.FromMinutes(3));
        }
        finally
        {
            if (!_idToTaskCompletionSourceMapping.Remove(id, out _))
            {
                _logger.LogError($"{nameof(SendAsync)} Failed to remove id from dictionary [{nameof(id)}={id}]");
            }
        }
    }

    public void ReceiveHttpResponse(ReceiveHttpResponseRequest receiveHttpResponseRequest)
    {
        var (id, response) = receiveHttpResponseRequest;
        if (!_idToTaskCompletionSourceMapping.TryGetValue(id, out var taskCompletionSource))
        {
            throw new KeyNotFoundException($"Could not locate request id [{nameof(id)}={id}]");
        }

        taskCompletionSource.SetResult(response);
    }
}