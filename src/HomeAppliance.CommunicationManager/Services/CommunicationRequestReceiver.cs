using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class CommunicationRequestReceiver : IHostedService
{
    private readonly ILogger<CommunicationRequestReceiver> _logger;
    private readonly ICommunicationServer _communicationServer;

    private readonly HubConnection _connection;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public CommunicationRequestReceiver(
        ILogger<CommunicationRequestReceiver> logger,
        IHostEnvironment hostEnvironment,
        ICommunicationServer communicationServer)
    {
        _logger = logger;
        _communicationServer = communicationServer;

        _cancellationTokenSource = new CancellationTokenSource();
        _connection =
            new HubConnectionBuilder().
                WithUrl(
                    "https://host.docker.internal:7050/Api/CommunicationRequestSender",
                    connectionOptions =>
                        connectionOptions.HttpMessageHandlerFactory =
                            httpMessageHandler =>
                            {
                                if (httpMessageHandler is HttpClientHandler httpClientHandler && hostEnvironment.IsDevelopment())
                                {
                                    httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                                }

                                return httpMessageHandler;
                            }).
                WithAutomaticReconnect().
                ConfigureLogging(
                    loggerBuilder =>
                    {
                        loggerBuilder.AddConsole();
                        loggerBuilder.SetMinimumLevel(LogLevel.Information);
                    }).
                AddNewtonsoftJsonProtocol(options => options.PayloadSerializerSettings.ConfigureCommon()).
                Build();
        ConfigureOnDisconnect();

        _connection.On<ICommunicationRequest>(
            SignalRMethods.ReceiveRequest,
            ReceiveRequestAsync);
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

    private void ConfigureOnDisconnect() =>
        _connection.Closed += exception =>
        {
            _logger.LogInformation($"client disconnected [{nameof(_connection.ConnectionId)}={_connection.ConnectionId} {nameof(exception)}={exception}]");
            return Task.CompletedTask;
        };

    private async Task ReceiveRequestAsync(ICommunicationRequest communicationRequest)
    {
        _logger.LogDebug($"{nameof(ReceiveRequestAsync)} started [{nameof(communicationRequest.Id)}={communicationRequest.Id} Type={communicationRequest.GetType().Name}]");

        await _communicationServer.ServeRequestAsync(communicationRequest, _cancellationTokenSource.Token);

        _logger.LogDebug($"{nameof(ReceiveRequestAsync)} finished [{nameof(communicationRequest.Id)}={communicationRequest.Id}]");
    }
}