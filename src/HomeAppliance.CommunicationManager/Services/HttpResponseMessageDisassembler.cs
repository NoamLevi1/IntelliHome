using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHttpResponseMessageDisassembler
{
    Task<HttpResponseData> DisassembleAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken);
}

public sealed class HttpResponseMessageDisassembler : IHttpResponseMessageDisassembler
{
    private readonly IRemoteContentManager _remoteContentManager;

    public HttpResponseMessageDisassembler(IRemoteContentManager remoteContentManager) =>
        _remoteContentManager = remoteContentManager;

    public async Task<HttpResponseData> DisassembleAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken) =>
        new()
        {
            ContentId = _remoteContentManager.AddContent(httpResponseMessage.Content),
            ContentHeaders = httpResponseMessage.Content.Headers,
            Headers = httpResponseMessage.Headers,
            ReasonPhrase = httpResponseMessage.ReasonPhrase,
            RequestData = httpResponseMessage.RequestMessage is null
                ? null
                : await HttpRequestData.FromHttpRequestMessageAsync(httpResponseMessage.RequestMessage, cancellationToken),
            Version = httpResponseMessage.Version,
            StatusCode = httpResponseMessage.StatusCode
        };
}