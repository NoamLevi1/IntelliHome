using System.Net.Http.Headers;

namespace IntelliHome.Common;

public static class HttpHeadersExtension
{
    public static void Aggregate(this HttpHeaders headers, IEnumerable<KeyValuePair<string, IEnumerable<string>>> other)
    {
        foreach (var (httpRequestHeaderKey, httpRequestHeaderValue) in other)
        {
            headers.Add(httpRequestHeaderKey, httpRequestHeaderValue);
        }
    }
}