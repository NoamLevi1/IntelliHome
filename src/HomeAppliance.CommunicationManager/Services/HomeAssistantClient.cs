using System.Net;
using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHomeAssistantClient
{
    Task<SendHomeAssistantHttpRequestResponse> SendAsync(SendHomeAssistantHttpRequestRequest homeAssistantHttpRequestMessage);
}

public sealed class HomeAssistantClient : IHomeAssistantClient
{
    private readonly ILogger<HomeAssistantClient> _logger;
    private readonly IHttpResponseMessageDisassembler _httpResponseMessageDisassembler;

    private readonly HttpMessageInvoker _httpMessageInvoker;

    public HomeAssistantClient(ILogger<HomeAssistantClient> logger, IHttpResponseMessageDisassembler httpResponseMessageDisassembler)
    {
        _logger = logger;
        _httpResponseMessageDisassembler = httpResponseMessageDisassembler;

        _httpMessageInvoker = new HttpMessageInvoker(
            new SocketsHttpHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });
    }

    public async Task<SendHomeAssistantHttpRequestResponse> SendAsync(SendHomeAssistantHttpRequestRequest homeAssistantHttpRequestMessage)
    {
        _logger.LogInformation($"{nameof(SendAsync)} started [{nameof(homeAssistantHttpRequestMessage.HttpRequestData.RequestUri)}={homeAssistantHttpRequestMessage.HttpRequestData.RequestUri} {nameof(homeAssistantHttpRequestMessage.HttpRequestData.Method)}={homeAssistantHttpRequestMessage.HttpRequestData.Method}]");

        var httpResponseMessage =
            await _httpMessageInvoker.
                SendAsync(homeAssistantHttpRequestMessage.HttpRequestData.ToHttpRequestMessage(), CancellationToken.None);

        _logger.LogInformation($"{nameof(SendAsync)} finished [{nameof(httpResponseMessage.StatusCode)}={httpResponseMessage.StatusCode}]");

        return new SendHomeAssistantHttpRequestResponse(
            await _httpResponseMessageDisassembler.DisassembleAsync(httpResponseMessage, CancellationToken.None));
    }
}