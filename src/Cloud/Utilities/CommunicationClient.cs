using IntelliHome.Common;

namespace IntelliHome.Cloud;

public interface ICommunicationClient
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TResponse : ICommunicationResponse
        where TRequest : IRequestWithResponse<TResponse>;

    async Task SendAsync(IVoidRequest request, CancellationToken cancellationToken) =>
        await SendAsync<IVoidRequest, VoidResponse>(request, cancellationToken);
}

public sealed class CommunicationClient : ICommunicationClient
{
    private readonly Guid _homeApplianceId;
    private readonly ICommunicationManager _communicationManager;

    public CommunicationClient(Guid homeApplianceId, ICommunicationManager communicationManager)
    {
        _homeApplianceId = homeApplianceId;
        _communicationManager = communicationManager;
    }

    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequestWithResponse<TResponse>
        where TResponse : ICommunicationResponse =>
        _communicationManager.SendAsync<TRequest, TResponse>(
            _homeApplianceId,
            request,
            cancellationToken);
}