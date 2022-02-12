using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class HomeApplianceHttpRequestMessageSenderHubClient : IHostedService
{
    private readonly ILogger<HomeApplianceHttpRequestMessageSenderHubClient> _logger;
    private readonly IHomeApplianceHttpResponseMessageReceiverClient _homeApplianceHttpResponseMessageReceiverClient;
    private readonly IHomeAssistantClient _homeAssistantClient;

    private readonly HubConnection _connection;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public HomeApplianceHttpRequestMessageSenderHubClient(
        ILogger<HomeApplianceHttpRequestMessageSenderHubClient> logger,
        IHomeAssistantClient homeAssistantClient,
        IHomeApplianceHttpResponseMessageReceiverClient homeApplianceHttpResponseMessageReceiverClient,
        IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _homeAssistantClient = homeAssistantClient;
        _homeApplianceHttpResponseMessageReceiverClient = homeApplianceHttpResponseMessageReceiverClient;

        _cancellationTokenSource = new CancellationTokenSource();
        _connection =
            new HubConnectionBuilder().
                WithUrl(
                    "https://host.docker.internal:7050/Api/HomeApplianceHttpRequestMessageSender",
                    connectionOptions =>
                        connectionOptions.HttpMessageHandlerFactory =
                            _ =>
                            {
                                var httpClientHandler = new HttpClientHandler();
                                if (hostEnvironment.IsDevelopment())
                                {
                                    httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                                }

                                return httpClientHandler;
                            }).
                WithAutomaticReconnect().
                AddNewtonsoftJsonProtocol(options => options.PayloadSerializerSettings.ConfigureCommon()).
                Build();
        ConfigureOnDisconnect();

        _connection.On<SendHttpRequestRequest>(
            HomeApplianceHttpRequestMessageSenderHubClientMethodNames.ReceiveHttpRequest,
            ReceiveHttpMessageAsync);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(StartAsync)} started");

        await _connection.StartAsync(cancellationToken);

        _logger.LogInformation($"{nameof(StartAsync)} finished");
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
            _logger.LogInformation($"client disconnected [{nameof(_connection.ConnectionId)}={_connection.ConnectionId} {nameof(exception)}={exception}]");
            return Task.CompletedTask;
        };
    }

    private async Task ReceiveHttpMessageAsync(SendHttpRequestRequest sendHttpRequestRequest)
    {
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP request received");
        var (id, httpRequestMessage) = sendHttpRequestRequest;
        var response = await _homeAssistantClient.SendAsync(httpRequestMessage, _cancellationTokenSource.Token);
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP Response received [{nameof(response.StatusCode)}={response.StatusCode}]");

        await _homeApplianceHttpResponseMessageReceiverClient.SendAsync(new ReceiveHttpResponseRequest(id, response), _cancellationTokenSource.Token);
        _logger.LogInformation($"{nameof(ReceiveHttpMessageAsync)} HTTP response sent back to server");
    }
}