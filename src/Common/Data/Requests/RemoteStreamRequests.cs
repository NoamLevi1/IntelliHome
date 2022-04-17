using Newtonsoft.Json;

namespace IntelliHome.Common;

public abstract class RemoteStreamRequest : CommunicationRequest
{
    [JsonProperty]
    public Guid StreamId { get; private set; }

    protected RemoteStreamRequest(Guid streamId) =>
        StreamId = streamId;
}

public sealed class RemoteStreamWriteRequest : RemoteStreamRequest, IVoidRequest
{
    [JsonProperty]
    public ReadOnlyMemory<byte> Buffer { get; private set; }

    public RemoteStreamWriteRequest(Guid streamId, ReadOnlyMemory<byte> buffer)
        : base(streamId) =>
        Buffer = buffer;
}

public sealed class RemoteStreamReadRequest : RemoteStreamRequest, IRequestWithResponse<RemoteStreamReadResponse>
{
    [JsonProperty]
    public int Count { get; private set; }

    public RemoteStreamReadRequest(Guid streamId, int count)
        : base(streamId) =>
        Count = count;
}

public sealed class RemoteStreamReadResponse : CommunicationResponse
{
    [JsonProperty]
    public ReadOnlyMemory<byte> Buffer { get; private set; }
    [JsonProperty]
    public int Result { get; private set; }

    public RemoteStreamReadResponse(ReadOnlyMemory<byte> buffer, int result)
    {
        Buffer = buffer;
        Result = result;
    }
}

public sealed class RemoteStreamFlushRequest : RemoteStreamRequest, IVoidRequest
{
    public RemoteStreamFlushRequest(Guid streamId)
        : base(streamId)
    {
    }
}

public sealed class RemoteStreamSetLengthRequest : RemoteStreamRequest, IVoidRequest
{
    [JsonProperty]
    public long Value { get; private set; }

    public RemoteStreamSetLengthRequest(Guid streamId, long value)
        : base(streamId) =>
        Value = value;
}

public sealed class RemoteStreamSeekRequest : RemoteStreamRequest, IRequestWithResponse<RemoteStreamSeekResponse>
{
    [JsonProperty]
    public long Offset { get; private set; }
    [JsonProperty]
    public SeekOrigin Origin { get; private set; }

    public RemoteStreamSeekRequest(Guid streamId, long offset, SeekOrigin origin)
        : base(streamId)
    {
        Offset = offset;
        Origin = origin;
    }
}

public sealed class RemoteStreamSeekResponse : CommunicationResponse
{
    [JsonProperty]
    public long Result { get; private set; }

    public RemoteStreamSeekResponse(long result) =>
        Result = result;
}

public sealed class RemoteStreamGetPropertiesRequest : RemoteStreamRequest, IRequestWithResponse<RemoteStreamGetPropertiesResponse>
{
    public RemoteStreamGetPropertiesRequest(Guid streamId)
        : base(streamId)
    {
    }
}

public sealed class RemoteStreamGetPropertiesResponse : CommunicationResponse
{
    [JsonProperty]
    public bool CanRead { get; private set; }
    [JsonProperty]
    public bool CanSeek { get; private set; }
    [JsonProperty]
    public bool CanWrite { get; private set; }
    [JsonProperty]
    public long Length { get; private set; }
    [JsonProperty]
    public long Position { get; private set; }

    public RemoteStreamGetPropertiesResponse(
        bool canRead,
        bool canSeek,
        bool canWrite,
        long length,
        long position)
    {
        CanRead = canRead;
        CanSeek = canSeek;
        CanWrite = canWrite;
        Length = length;
        Position = position;
    }
}

public sealed class RemoteStreamSetPositionRequest : RemoteStreamRequest, IVoidRequest
{
    [JsonProperty]
    public long Value { get; private set; }

    public RemoteStreamSetPositionRequest(Guid streamId, long value)
        : base(streamId) =>
        Value = value;
}

public sealed class RemoteStreamDisposeRequest : RemoteStreamRequest, IVoidRequest
{
    public RemoteStreamDisposeRequest(Guid streamId)
        : base(streamId)
    {
    }
}