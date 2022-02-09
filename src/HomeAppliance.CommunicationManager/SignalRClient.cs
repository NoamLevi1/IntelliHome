using Microsoft.AspNetCore.SignalR.Client;

namespace IntelliHome.HomeAppliance.CommunicationManager;
public class SignalRClient : IHostedService
{
    private readonly ILogger<SignalRClient> _logger;
    private readonly IHttpMessageHandler _httpMessageHandler;

    private readonly HubConnection _connection;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public SignalRClient(ILogger<SignalRClient> logger, IHttpMessageHandler httpMessageHandler)
    {
        _logger = logger;
        _httpMessageHandler = httpMessageHandler;

        _cancellationTokenSource = new CancellationTokenSource();
        _connection = new HubConnectionBuilder().WithUrl("Https://localhost:7050/Connectionhub").WithAutomaticReconnect().Build();
        ConfigureOnDisconnect();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection.On<HttpRequestMessage>(
            nameof(ReceiveHttpMessageAsync),
            ReceiveHttpMessageAsync);

        await _connection.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        await _connection.StopAsync(cancellationToken);
    }

    private void ConfigureOnDisconnect()
    {
        _connection.Closed += exception =>
        {
            _logger.LogInformation($"{nameof(ConfigureOnDisconnect)} Exception {exception}");
            return Task.CompletedTask;
        };
    }

    private async Task ReceiveHttpMessageAsync(HttpRequestMessage httpRequestMessage)
    {
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP request received");
        var response = await _httpMessageHandler.SendAsync(httpRequestMessage, _cancellationTokenSource.Token);
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP Response received wth [{nameof(response.StatusCode)}={response.StatusCode}]");

        await _connection.InvokeAsync<HttpResponseMessage>("SendResponseBackToServer", response,_cancellationTokenSource.Token);
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP response sent back to server");
    }
}