using IntelliHome.Common;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHomeApplianceHttpResponseMessageReceiverClient
{
    Task SendAsync(ReceiveHttpResponseRequest receiveHttpResponseRequest, CancellationToken cancellationToken);
}

public sealed class HomeApplianceHttpResponseMessageReceiverClient : IHomeApplianceHttpResponseMessageReceiverClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializer _serializer;

    private readonly ILogger<HomeApplianceHttpResponseMessageReceiverClient> _logger;

    public HomeApplianceHttpResponseMessageReceiverClient(ILogger<HomeApplianceHttpResponseMessageReceiverClient> logger, IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _httpClient = hostEnvironment.IsDevelopment()
            ? new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
            : new HttpClient();

        _serializer = JsonSerializer.Create(new JsonSerializerSettings().ConfigureCommon());
    }

    public async Task SendAsync(ReceiveHttpResponseRequest receiveHttpResponseRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendAsync)} started [{nameof(receiveHttpResponseRequest.Id)}={receiveHttpResponseRequest.Id}]");

        (await _httpClient.PostAsync(
                "https://host.docker.internal:7050/Api/HomeApplianceHttpResponseMessageReceiver",
                new StringContent(
                    _serializer.SerializeToString(receiveHttpResponseRequest),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
                cancellationToken)).
            EnsureSuccessStatusCode();

        _logger.LogInformation($"{nameof(SendAsync)} finished [{nameof(receiveHttpResponseRequest.Id)}={receiveHttpResponseRequest.Id}]");
    }
}