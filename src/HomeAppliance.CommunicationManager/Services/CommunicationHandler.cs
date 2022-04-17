using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface ICommunicationHandler
{
    Task<ICommunicationResponse> HandleRequestAsync(ICommunicationRequest request);
}

public sealed class CommunicationHandler : ICommunicationHandler
{
    private readonly Dictionary<Type, Func<ICommunicationRequest, Task<ICommunicationResponse>>> _requestTypeToHandlerMapping;

    public CommunicationHandler(
        IHomeAssistantClient homeAssistantClient,
        IRemoteStreamManager remoteStreamManager,
        IRemoteContentManager remoteContentManager)
    {
        _requestTypeToHandlerMapping = new Dictionary<Type, Func<ICommunicationRequest, Task<ICommunicationResponse>>>();

        Register<SendHomeAssistantHttpRequestRequest, SendHomeAssistantHttpRequestResponse>(homeAssistantClient.SendAsync);
        Register<CreateContentReadStreamRemoteContentRequest, CreateContentReadStreamRemoteContentResponse>(remoteContentManager.CreateContentReadStreamAsync);
        Register<DisposeRemoteContentRequest>(remoteContentManager.Dispose);
        Register<RemoteStreamWriteRequest>(remoteStreamManager.WriteAsync);
        Register<RemoteStreamReadRequest, RemoteStreamReadResponse>(remoteStreamManager.ReadAsync);
        Register<RemoteStreamGetPropertiesRequest, RemoteStreamGetPropertiesResponse>(remoteStreamManager.GetProperties);
        Register<RemoteStreamSeekRequest, RemoteStreamSeekResponse>(remoteStreamManager.Seek);
        Register<RemoteStreamFlushRequest>(remoteStreamManager.FlushAsync);
        Register<RemoteStreamSetLengthRequest>(remoteStreamManager.SetLength);
        Register<RemoteStreamSetPositionRequest>(remoteStreamManager.SetPosition);
        Register<RemoteStreamDisposeRequest>(remoteStreamManager.DisposeAsync);
    }

    public Task<ICommunicationResponse> HandleRequestAsync(ICommunicationRequest request) =>
        _requestTypeToHandlerMapping[request.GetType()](request);

    private void Register<TRequest>(Action<TRequest> handleRequest)
        where TRequest : IVoidRequest =>
        Register<TRequest, VoidResponse>(
            request =>
            {
                handleRequest(request);
                return new VoidResponse();
            });

    private void Register<TRequest, TResponse>(Func<TRequest, TResponse> handleRequest)
        where TRequest : IRequestWithResponse<TResponse>
        where TResponse : ICommunicationResponse =>
        Register<TRequest, TResponse>(request => Task.Run(() => handleRequest(request)));

    private void Register<TRequest>(Func<TRequest, Task> handleRequestAsync)
        where TRequest : IVoidRequest =>
        Register<TRequest, VoidResponse>(
            async request =>
            {
                await handleRequestAsync(request);
                return new VoidResponse();
            });

    private void Register<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handleRequestAsync)
        where TRequest : IRequestWithResponse<TResponse>
        where TResponse : ICommunicationResponse
    {
        if (!_requestTypeToHandlerMapping.TryAdd(typeof(TRequest), async communicationRequest => await handleRequestAsync((TRequest) communicationRequest)))
        {
            throw new Exception($"Request handler is already registered [{nameof(TRequest)}={typeof(TRequest).Name}]");
        }
    }
}