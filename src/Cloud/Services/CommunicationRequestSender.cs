using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud;

public interface ICommunicationRequestSender
{
    Task SendRequestAsync(
        Guid homeApplianceId,
        ICommunicationRequest communicationRequest,
        CancellationToken cancellationToken);
}

public sealed class CommunicationRequestSender : Hub, ICommunicationRequestSender
{
    private readonly ILogger<CommunicationRequestSender> _logger;
    private readonly IHubContext<CommunicationRequestSender> _hubContext;
    private readonly IHomeApplianceStore _homeApplianceStore;

    public CommunicationRequestSender(
        ILogger<CommunicationRequestSender> logger,
        IHubContext<CommunicationRequestSender> hubContext,
        IHomeApplianceStore homeApplianceStore)
    {
        _logger = logger;
        _hubContext = hubContext;
        _homeApplianceStore = homeApplianceStore;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"{nameof(OnConnectedAsync)} Connection established [{nameof(Context.ConnectionId)}={Context.ConnectionId}]");
        _homeApplianceStore.AddOrUpdateHomeAppliance(Guid.Empty, Context.ConnectionId);

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            $"{nameof(OnConnectedAsync)} Connection terminated " +
            $"[{nameof(Context.ConnectionId)}={Context.ConnectionId} " +
            $"{nameof(exception)}={exception}]");

        return Task.CompletedTask;
    }

    public async Task SendRequestAsync(
        Guid homeApplianceId,
        ICommunicationRequest communicationRequest,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(SendRequestAsync)} started [{nameof(communicationRequest.Id)}={communicationRequest.Id}]");

        await _hubContext.
            Clients.
            Client(_homeApplianceStore.GetConnectionId(homeApplianceId)).
            SendAsync(
                SignalRMethods.ReceiveRequest,
                communicationRequest,
                cancellationToken);

        _logger.LogDebug($"{nameof(SendRequestAsync)} finished");
    }
}