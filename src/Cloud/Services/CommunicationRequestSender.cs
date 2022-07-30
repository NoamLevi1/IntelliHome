using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

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
    private readonly IDatabase _database;
    private readonly IHubContext<CommunicationRequestSender> _hubContext;

    private Guid HomeApplianceId => Guid.Empty;

    public CommunicationRequestSender(
        ILogger<CommunicationRequestSender> logger,
        IDatabase database,
        IHubContext<CommunicationRequestSender> hubContext)
    {
        _logger = logger;
        _database = database;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"{nameof(OnConnectedAsync)} Connection established [{nameof(Context.ConnectionId)}={Context.ConnectionId}]");

        await _database.HomeAppliances.UpdateOneAsync(
            homeAppliance => homeAppliance.Id == HomeApplianceId,
            new UpdateDefinitionBuilder<HomeAppliance>().Set(homeAppliance => homeAppliance.ConnectionId, Context.ConnectionId),
            new UpdateOptions {IsUpsert = true});
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            $"{nameof(OnConnectedAsync)} Connection terminated " +
            $"[{nameof(Context.ConnectionId)}={Context.ConnectionId} " +
            $"{nameof(exception)}={exception}]");

        await _database.HomeAppliances.UpdateOneAsync(
            homeAppliance => homeAppliance.Id == HomeApplianceId,
            new UpdateDefinitionBuilder<HomeAppliance>().Set(homeAppliance => homeAppliance.ConnectionId, null));
    }

    public async Task SendRequestAsync(
        Guid homeApplianceId,
        ICommunicationRequest communicationRequest,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(SendRequestAsync)} started [{nameof(communicationRequest.Id)}={communicationRequest.Id}]");

        var connectionId =
            (await (await _database.
                    HomeAppliances.
                    FindAsync(
                        appliance => appliance.Id == homeApplianceId,
                        cancellationToken: cancellationToken)).
                SingleAsync(cancellationToken)).
            ConnectionId;

        if (connectionId == null)
        {
            throw new Exception($"Cannot send request to a disconnected home appliance [{nameof(homeApplianceId)}={homeApplianceId}]");
        }

        await _hubContext.
            Clients.
            Client(connectionId).
            SendAsync(
                SignalRMethods.ReceiveRequest,
                communicationRequest,
                cancellationToken);

        _logger.LogDebug($"{nameof(SendRequestAsync)} finished");
    }
}