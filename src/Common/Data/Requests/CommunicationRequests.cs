using Newtonsoft.Json;

namespace IntelliHome.Common;

public interface ICommunicationRequest
{
    public Guid Id { get; }
}

public interface ICommunicationResponse
{
    public Guid RequestId { get; set; }
}

public interface IRequestWithResponse<TResponse> : ICommunicationRequest
    where TResponse : ICommunicationResponse
{
}

public interface IVoidRequest : IRequestWithResponse<VoidResponse>
{
}

public abstract class CommunicationRequest : ICommunicationRequest
{
    [JsonProperty]
    public Guid Id { get; private set; }

    protected CommunicationRequest() =>
        Id = Guid.NewGuid();
}

public abstract class CommunicationResponse : ICommunicationResponse
{
    public Guid RequestId { get; set; }
}

public sealed class VoidResponse : CommunicationResponse
{
}

public sealed class ExceptionResponse : CommunicationResponse
{
    [JsonProperty]
    public Exception Exception { get; private set; }

    public ExceptionResponse(Exception exception) =>
        Exception = exception;
}