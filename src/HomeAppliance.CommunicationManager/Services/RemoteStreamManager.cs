using System.Collections.Concurrent;
using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IRemoteStreamManager
{
    Guid AddStream(Stream stream);
    Task WriteAsync(RemoteStreamWriteRequest request);
    Task<RemoteStreamReadResponse> ReadAsync(RemoteStreamReadRequest request);
    Task FlushAsync(RemoteStreamFlushRequest request);
    void SetLength(RemoteStreamSetLengthRequest request);
    RemoteStreamSeekResponse Seek(RemoteStreamSeekRequest request);
    RemoteStreamGetPropertiesResponse GetProperties(RemoteStreamGetPropertiesRequest request);
    void SetPosition(RemoteStreamSetPositionRequest request);
    Task DisposeAsync(RemoteStreamDisposeRequest request);
}

public sealed class RemoteStreamManager : IRemoteStreamManager
{
    private readonly ConcurrentDictionary<Guid, Stream> _streamIdToStreamMapping;

    public RemoteStreamManager() =>
        _streamIdToStreamMapping = new ConcurrentDictionary<Guid, Stream>();

    public Guid AddStream(Stream stream)
    {
        var id = Guid.NewGuid();

        if (!_streamIdToStreamMapping.TryAdd(id, stream))
        {
            throw new Exception($"Stream id already exist [{nameof(id)}={id}]");
        }

        return id;
    }

    public async Task WriteAsync(RemoteStreamWriteRequest request) =>
        await GetStream(request.StreamId).WriteAsync(request.Buffer);

    public async Task<RemoteStreamReadResponse> ReadAsync(RemoteStreamReadRequest request)
    {
        var buffer = new byte[request.Count];
        var result = await GetStream(request.StreamId).ReadAsync(buffer);
        return new RemoteStreamReadResponse(new ReadOnlyMemory<byte>(buffer, 0, result), result);
    }

    public async Task FlushAsync(RemoteStreamFlushRequest request) =>
        await GetStream(request.StreamId).FlushAsync();

    public void SetLength(RemoteStreamSetLengthRequest request) =>
        GetStream(request.StreamId).SetLength(request.Value);

    public RemoteStreamSeekResponse Seek(RemoteStreamSeekRequest request) =>
        new (GetStream(request.StreamId).Seek(request.Offset, request.Origin));

    public RemoteStreamGetPropertiesResponse GetProperties(RemoteStreamGetPropertiesRequest request)
    {
        var stream = GetStream(request.StreamId);

        return new RemoteStreamGetPropertiesResponse(
            stream.CanRead,
            stream.CanSeek,
            stream.CanWrite,
            stream.Length,
            stream.Position);
    }

    public void SetPosition(RemoteStreamSetPositionRequest request) =>
        GetStream(request.StreamId).Position = request.Value;

    public async Task DisposeAsync(RemoteStreamDisposeRequest request)
    {
        await GetStream(request.StreamId).DisposeAsync();

        _streamIdToStreamMapping.TryRemove(request.StreamId, out _);
    }

    private Stream GetStream(Guid id) =>
        _streamIdToStreamMapping.GetValueOrDefault(id) ?? throw new Exception($"Unable to find stream [{nameof(id)}={id}]");
}