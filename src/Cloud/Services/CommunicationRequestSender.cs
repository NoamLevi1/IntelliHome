using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud;

public interface ICommunicationRequestSender
{
    Task SendRequestAsync(ICommunicationRequest communicationRequest, CancellationToken cancellationToken);
}

public sealed class CommunicationRequestSender : Hub, ICommunicationRequestSender
{
    private readonly ILogger<CommunicationRequestSender> _logger;
    private readonly IHubContext<CommunicationRequestSender> _hubContext;
    private readonly IClientStore _clientStore;

    public CommunicationRequestSender(
        ILogger<CommunicationRequestSender> logger,
        IHubContext<CommunicationRequestSender> hubContext,
        IClientStore clientStore)
    {
        _logger = logger;
        _hubContext = hubContext;
        _clientStore = clientStore;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"{nameof(OnConnectedAsync)} Connection established [{nameof(Context.ConnectionId)}={Context.ConnectionId}]");
        _clientStore.Clients.Add(Context.ConnectionId);

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            $"{nameof(OnConnectedAsync)} Connection terminated " +
            $"[{nameof(Context.ConnectionId)}={Context.ConnectionId} " +
            $"{nameof(exception)}={exception}]");
        _clientStore.Clients.Remove(Context.ConnectionId);

        return Task.CompletedTask;
    }

    public async Task SendRequestAsync(ICommunicationRequest communicationRequest, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(SendRequestAsync)} started [{nameof(communicationRequest.Id)}={communicationRequest.Id}]");

        await _hubContext.
            Clients.
            Client(_clientStore.Clients.Single()).
            SendAsync(
                SignalRMethods.ReceiveRequest,
                communicationRequest,
                cancellationToken);

        _logger.LogDebug($"{nameof(SendRequestAsync)} finished");
    }
}