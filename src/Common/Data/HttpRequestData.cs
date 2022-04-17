namespace IntelliHome.Common;

public sealed class HttpRequestData
{
    public byte[]? ContentBytes { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ContentHeaders { get; set; }
    public Uri? RequestUri { get; set; }
    public HttpMethod? Method { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? Headers { get; set; }
    public IDictionary<string, object?>? Options { get; set; }
    public HttpVersionPolicy VersionPolicy { get; set; }
    public Version? Version { get; set; }

    private HttpRequestData()
    {
    }

    public HttpRequestMessage ToHttpRequestMessage()
    {
        Ensure.NotNull(Method);
        Ensure.NotNull(Version);
        Ensure.NotNull(Headers);
        Ensure.NotNull(Options);

        var httpRequestMessage = new HttpRequestMessage(Method, RequestUri)
        {
            Content = ContentBytes is not null
                ? new ByteArrayContent(ContentBytes)
                : null,
            Version = Version,
            VersionPolicy = VersionPolicy
        };

        httpRequestMessage.Headers.Aggregate(Headers);
        httpRequestMessage.Options.Aggregate(Options);

        if (httpRequestMessage.Content is not null && ContentHeaders is not null)
        {
            httpRequestMessage.Content.Headers.Aggregate(ContentHeaders);
        }

        return httpRequestMessage;
    }

    public static async Task<HttpRequestData> FromHttpRequestMessageAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        var data = new HttpRequestData
        {
            Headers = httpRequestMessage.Headers,
            Options = httpRequestMessage.Options,
            Version = httpRequestMessage.Version,
            VersionPolicy = httpRequestMessage.VersionPolicy,
            RequestUri = httpRequestMessage.RequestUri,
            Method = httpRequestMessage.Method
        };

        if (httpRequestMessage.Content is not null)
        {
            data.ContentBytes = await httpRequestMessage.Content.ReadAsByteArrayAsync(cancellationToken);
            data.ContentHeaders = httpRequestMessage.Content.Headers;
        }

        return data;
    }
}