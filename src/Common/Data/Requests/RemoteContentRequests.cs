using Newtonsoft.Json;

namespace IntelliHome.Common;

public abstract class RemoteContentRequest : CommunicationRequest
{
    [JsonProperty]
    public Guid ContentId { get; private set; }

    protected RemoteContentRequest(Guid contentId) =>
        ContentId = contentId;
}

public sealed class CreateContentReadStreamRemoteContentRequest : RemoteContentRequest, IRequestWithResponse<CreateContentReadStreamRemoteContentResponse>
{
    public CreateContentReadStreamRemoteContentRequest(Guid contentId)
        : base(contentId)
    {
    }
}

public sealed class CreateContentReadStreamRemoteContentResponse : CommunicationResponse
{
    [JsonProperty]
    public Guid StreamId { get; private set; }

    public CreateContentReadStreamRemoteContentResponse(Guid streamId) =>
        StreamId = streamId;
}

public sealed class DisposeRemoteContentRequest : RemoteContentRequest, IVoidRequest
{
    public DisposeRemoteContentRequest(Guid contentId)
        : base(contentId)
    {
    }
}