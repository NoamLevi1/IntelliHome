using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud;

public interface IHomeApplianceHttpMessageSender
{
    Task SendMessageAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
}

public sealed class HomeApplianceHttpMessageSender : Hub, IHomeApplianceHttpMessageSender
{
    private readonly ILogger<HomeApplianceHttpMessageSender> _logger;

    public HomeApplianceHttpMessageSender(ILogger<HomeApplianceHttpMessageSender> logger) =>
        _logger = logger;

    public async Task SendMessageAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        Ensure.NotNull(httpRequestMessage);

        _logger.LogInformation($"{nameof(SendMessageAsync)} started");

        await Clients.All.SendAsync("ReceiveHttpMessage", httpRequestMessage, cancellationToken);

        _logger.LogInformation($"{nameof(SendMessageAsync)} started");
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"{nameof(OnConnectedAsync)} Connection established [{nameof(Context.ConnectionId)}={Context.ConnectionId}]");

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            $"{nameof(OnConnectedAsync)} Connection terminated " +
            $"[{nameof(Context.ConnectionId)}={Context.ConnectionId}" +
            $"{nameof(exception)}={exception}]");

        return Task.CompletedTask;
    }
}