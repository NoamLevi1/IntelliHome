namespace IntelliHome.Common;

public sealed class SendHomeAssistantHttpRequestRequest : CommunicationRequest, IRequestWithResponse<SendHomeAssistantHttpRequestResponse>
{
    public HttpRequestData HttpRequestData { get; }

    public SendHomeAssistantHttpRequestRequest(HttpRequestData httpRequestData) =>
        HttpRequestData = httpRequestData;
}

public sealed class SendHomeAssistantHttpRequestResponse : CommunicationResponse
{
    public HttpResponseData HttpResponseData { get; }

    public SendHomeAssistantHttpRequestResponse(HttpResponseData httpResponseData) =>
        HttpResponseData = httpResponseData;
}