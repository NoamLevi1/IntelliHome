using IntelliHome.Common;

namespace IntelliHome.Cloud;

public sealed class RemoteStream : Stream
{
    private readonly Guid _id;
    private readonly ICommunicationClient _communicationClient;

    public override bool CanRead => GetProperties().CanRead;
    public override bool CanSeek => GetProperties().CanSeek;
    public override bool CanWrite => GetProperties().CanWrite;
    public override long Length => GetProperties().Length;
    public override long Position
    {
        get => GetProperties().Position;
        set => _communicationClient.SendAsync(new RemoteStreamSetPositionRequest(_id, value), CancellationToken.None).Await();
    }

    public RemoteStream(Guid id, ICommunicationClient communicationClient)
    {
        _id = id;
        _communicationClient = communicationClient;
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        _communicationClient.
            SendAsync<RemoteStreamSeekRequest, RemoteStreamSeekResponse>(new RemoteStreamSeekRequest(_id, offset, origin), CancellationToken.None).
            Await().
            Result;

    public override void SetLength(long value) =>
        _communicationClient.SendAsync(new RemoteStreamSetLengthRequest(_id, value), CancellationToken.None).Await();

    public override void Write(byte[] buffer, int offset, int count) =>
        WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count)).Await();

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => 
        await _communicationClient.SendAsync(new RemoteStreamWriteRequest(_id, buffer), cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAsync(new Memory<byte>(buffer, offset, count)).Await();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
        await ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken);

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var remoteStreamReadResponse =
            await _communicationClient.SendAsync<RemoteStreamReadRequest, RemoteStreamReadResponse>(
                new RemoteStreamReadRequest(_id, buffer.Length),
                cancellationToken);

        remoteStreamReadResponse.Buffer.CopyTo(buffer);

        return remoteStreamReadResponse.Result;
    }

    public override void Flush() =>
        FlushAsync().Await();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _communicationClient.SendAsync(new RemoteStreamFlushRequest(_id), cancellationToken);

    public override async ValueTask DisposeAsync() =>
        await _communicationClient.SendAsync(new RemoteStreamDisposeRequest(_id), CancellationToken.None);

    protected override void Dispose(bool disposing) =>
        DisposeAsync().Await();

    private RemoteStreamGetPropertiesResponse GetProperties() =>
        _communicationClient.
            SendAsync<RemoteStreamGetPropertiesRequest, RemoteStreamGetPropertiesResponse>(new RemoteStreamGetPropertiesRequest(_id), CancellationToken.None).
            Await();
}