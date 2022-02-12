namespace IntelliHome.Common;

public sealed record SendHttpRequestRequest(Guid Id, HttpRequestMessage Request);
public sealed record ReceiveHttpResponseRequest(Guid Id, HttpResponseMessage Response);