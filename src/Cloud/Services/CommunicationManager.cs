using System.Collections.Concurrent;
using IntelliHome.Common;

namespace IntelliHome.Cloud;

public interface ICommunicationManager
{
    Task<TResponse> SendAsync<TRequest, TResponse>(
        Guid homeApplianceId,
        TRequest request,
        CancellationToken cancellationToken)
        where TResponse : ICommunicationResponse
        where TRequest : IRequestWithResponse<TResponse>;

    void SetResponse(ICommunicationResponse communicationResponse);
}

public sealed class CommunicationManager : ICommunicationManager
{
    private readonly ILogger<CommunicationManager> _logger;
    private readonly ICommunicationRequestSender _communicationRequestSender;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ICommunicationResponse>> _idToTaskCompletionSourceMapping;

    public CommunicationManager(ILogger<CommunicationManager> logger, ICommunicationRequestSender communicationRequestSender)
    {
        _logger = logger;
        _communicationRequestSender = communicationRequestSender;
        _idToTaskCompletionSourceMapping = new ConcurrentDictionary<Guid, TaskCompletionSource<ICommunicationResponse>>();
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(
        Guid homeApplianceId,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequestWithResponse<TResponse>
        where TResponse : ICommunicationResponse
    {
        _logger.LogDebug($"{nameof(SendAsync)} Started [{nameof(request.Id)}={request.Id} Type={request.GetType().Name}]");

        ICommunicationResponse communicationResponse;
        var taskCompletionSource = new TaskCompletionSource<ICommunicationResponse>();

        if (!_idToTaskCompletionSourceMapping.TryAdd(request.Id, taskCompletionSource))
        {
            throw new Exception($"Id already exists [{nameof(request.Id)}={request.Id}]");
        }

        try
        {
            await _communicationRequestSender.SendRequestAsync(homeApplianceId, request, cancellationToken);
            await using (cancellationToken.Register(() => taskCompletionSource.SetCanceled(cancellationToken)))
            {
                communicationResponse = await taskCompletionSource.Task;
            }
        }
        finally
        {
            if (!_idToTaskCompletionSourceMapping.Remove(request.Id, out _))
            {
                _logger.LogError($"{nameof(SendAsync)} Failed to remove id from dictionary [{nameof(request.Id)}={request.Id}]");
            }
        }

        _logger.LogDebug($"{nameof(SendAsync)} Finished [{nameof(request.Id)}={request.Id}]");

        return (TResponse) communicationResponse;
    }

    public void SetResponse(ICommunicationResponse communicationResponse)
    {
        _logger.LogDebug($"{nameof(SetResponse)} started [{nameof(communicationResponse.RequestId)}={communicationResponse.RequestId}]");

        if (!_idToTaskCompletionSourceMapping.TryGetValue(communicationResponse.RequestId, out var taskCompletionSource))
        {
            throw new KeyNotFoundException($"Could not locate request id [{nameof(communicationResponse.RequestId)}={communicationResponse.RequestId}]");
        }

        if (communicationResponse is ExceptionResponse exceptionResponse)
        {
            taskCompletionSource.SetException(exceptionResponse.Exception);
        }
        else
        {
            Task.Run(() => taskCompletionSource.SetResult(communicationResponse));
        }

        _logger.LogDebug($"{nameof(SetResponse)} finished [{nameof(communicationResponse.RequestId)}={communicationResponse.RequestId}]");
    }
}