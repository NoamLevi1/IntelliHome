using System.Net;

namespace IntelliHome.Common;

public sealed class HttpResponseData
{
    public Guid ContentId { get; set; }
    public Version? Version { get; set; }
    public HttpRequestData? RequestData { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ContentHeaders { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? Headers { get; set; }
}