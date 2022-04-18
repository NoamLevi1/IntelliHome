using IntelliHome.Common;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface ICommunicationResponseSender
{
    Task SendResponseAsync(ICommunicationResponse communicationResponse, CancellationToken cancellationToken);
}

public sealed class CommunicationResponseSender : ICommunicationResponseSender
{
    private readonly Uri _communicationResponseReceiverUrl;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializer _serializer;

    private readonly ILogger<CommunicationResponseSender> _logger;

    public CommunicationResponseSender(
        ILogger<CommunicationResponseSender> logger,
        IHostEnvironment hostEnvironment,
        ICloudUrlBuilder cloudUrlBuilder)
    {
        _logger = logger;

        _communicationResponseReceiverUrl = cloudUrlBuilder.GetCommunicationResponseReceiverUri();
        _httpClient = hostEnvironment.IsDevelopment()
            ? new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
            : new HttpClient();
        _serializer = JsonSerializer.Create(new JsonSerializerSettings().ConfigureCommon());
    }

    public async Task SendResponseAsync(ICommunicationResponse communicationResponse, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(SendResponseAsync)} started [{nameof(communicationResponse.RequestId)}={communicationResponse.RequestId}]");

        (await _httpClient.PostAsync(
                _communicationResponseReceiverUrl,
                new StringContent(
                    _serializer.SerializeToString(communicationResponse),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
                cancellationToken)).
            EnsureSuccessStatusCode();

        _logger.LogDebug($"{nameof(SendResponseAsync)} finished [{nameof(communicationResponse.RequestId)}={communicationResponse.RequestId}]");
    }
}