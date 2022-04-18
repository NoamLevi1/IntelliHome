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
        ICloudUrlBuilder cloudUrlBuilder,
        ICommunicationServer communicationServer)
    {
        _logger = logger;
        _communicationServer = communicationServer;

        _cancellationTokenSource = new CancellationTokenSource();
        _connection =
            new HubConnectionBuilder().
                WithUrl(
                    cloudUrlBuilder.GetCommunicationRequestSenderUri(),
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
            ReceiveRequest);
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

    private void ReceiveRequest(ICommunicationRequest communicationRequest)
    {
        _logger.LogDebug($"{nameof(ReceiveRequest)} started [{nameof(communicationRequest.Id)}={communicationRequest.Id} Type={communicationRequest.GetType().Name}]");

        _communicationServer.ServeRequestAsync(communicationRequest, _cancellationTokenSource.Token);

        _logger.LogDebug($"{nameof(ReceiveRequest)} finished [{nameof(communicationRequest.Id)}={communicationRequest.Id}]");
    }
}