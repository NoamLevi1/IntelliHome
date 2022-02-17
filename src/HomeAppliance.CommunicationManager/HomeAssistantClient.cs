namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHomeAssistantClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
}

public sealed class HomeAssistantClient : IHomeAssistantClient
{
    private readonly ILogger<HomeAssistantClient> _logger;
    private readonly HttpClient _client;

    public HomeAssistantClient(ILogger<HomeAssistantClient> logger)
    {
        _logger = logger;
        _client = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendAsync)} started [{nameof(httpRequestMessage.RequestUri)}={httpRequestMessage.RequestUri}]");

        var httpResponseMessage = await _client.SendAsync(httpRequestMessage, cancellationToken);

        _logger.LogInformation($"{nameof(SendAsync)} finished [{nameof(httpResponseMessage.StatusCode)}={httpResponseMessage.StatusCode}]");

        return httpResponseMessage;
    }
}