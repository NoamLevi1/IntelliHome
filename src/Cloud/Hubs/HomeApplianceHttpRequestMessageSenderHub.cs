using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud;

public interface IHomeApplianceHttpRequestMessageSenderHub
{
    Task SendRequestAsync(SendHttpRequestRequest sendHttpRequestRequest, CancellationToken cancellationToken);
}

public sealed class HomeApplianceHttpRequestMessageSenderHub : Hub, IHomeApplianceHttpRequestMessageSenderHub
{
    private readonly ILogger<HomeApplianceHttpRequestMessageSenderHub> _logger;
    private readonly IHubContext<HomeApplianceHttpRequestMessageSenderHub> _hubContext;
    private readonly IClientStore _clientStore;

    public HomeApplianceHttpRequestMessageSenderHub(
        ILogger<HomeApplianceHttpRequestMessageSenderHub> logger,
        IHubContext<HomeApplianceHttpRequestMessageSenderHub> hubContext,
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

    public async Task SendRequestAsync(SendHttpRequestRequest sendHttpRequestRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendRequestAsync)} started");

        await _hubContext.
            Clients.
            Client(_clientStore.Clients.Single()).
            SendAsync(
                HomeApplianceHttpRequestMessageSenderHubClientMethodNames.ReceiveHttpRequest,
                sendHttpRequestRequest,
                cancellationToken);

        _logger.LogInformation($"{nameof(SendRequestAsync)} finished");
    }
}