namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHttpMessageHandler
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
}

public sealed class HttpMessageHandler : IHttpMessageHandler
{
    private readonly ILogger<HttpMessageHandler> _logger;
    private readonly HttpClient _client;

    public HttpMessageHandler(ILogger<HttpMessageHandler> logger)
    {
        _logger = logger;
        _client = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendAsync)} started");

        var httpResponseMessage = await _client.SendAsync(httpRequestMessage, cancellationToken);

        _logger.LogInformation($"{nameof(SendAsync)} finished");

        return httpResponseMessage;
    }
}