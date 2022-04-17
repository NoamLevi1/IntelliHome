using IntelliHome.Common;

namespace IntelliHome.Cloud;

public interface IHttpResponseMessageBuilder
{
    HttpResponseMessage Build(HttpResponseData httpResponseData);
}

public sealed class HttpResponseMessageBuilder : IHttpResponseMessageBuilder
{
    private readonly ICommunicationClient _communicationClient;

    public HttpResponseMessageBuilder(ICommunicationClient communicationClient) =>
        _communicationClient = communicationClient;

    public HttpResponseMessage Build(HttpResponseData httpResponseData)
    {
        Ensure.NotNull(httpResponseData.Headers);
        Ensure.NotNull(httpResponseData.Version);
        Ensure.NotNull(httpResponseData.ContentHeaders);

        var response = new HttpResponseMessage(httpResponseData.StatusCode)
        {
            ReasonPhrase = httpResponseData.ReasonPhrase,
            RequestMessage = httpResponseData.RequestData?.ToHttpRequestMessage(),
            Version = httpResponseData.Version,
            Content = new RemoteContent(httpResponseData.ContentId, _communicationClient)
        };

        response.Headers.Aggregate(httpResponseData.Headers);
        response.Content.Headers.Aggregate(httpResponseData.ContentHeaders);

        return response;
    }
}