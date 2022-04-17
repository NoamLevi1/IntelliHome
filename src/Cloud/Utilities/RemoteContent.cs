using System.Net;
using IntelliHome.Common;

namespace IntelliHome.Cloud;

public sealed class RemoteContent : HttpContent
{
    private readonly Guid _id;
    private readonly ICommunicationClient _communicationClient;

    public RemoteContent(Guid id, ICommunicationClient communicationClient)
    {
        _id = id;
        _communicationClient = communicationClient;
    }

    protected override Stream CreateContentReadStream(CancellationToken cancellationToken) =>
        CreateContentReadStreamAsync(cancellationToken).Await();

    protected override Task<Stream> CreateContentReadStreamAsync() =>
        CreateContentReadStreamAsync(CancellationToken.None);

    protected override async Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        var response =
            await _communicationClient.SendAsync<CreateContentReadStreamRemoteContentRequest, CreateContentReadStreamRemoteContentResponse>(
                new CreateContentReadStreamRemoteContentRequest(_id),
                cancellationToken);

        return new RemoteStream(response.StreamId, _communicationClient);
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
        SerializeToStreamAsync(stream, context, cancellationToken).Await();

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        await SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
        await (await CreateContentReadStreamAsync(cancellationToken)).CopyToAsync(stream, cancellationToken);

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

    protected override void Dispose(bool disposing) =>
        _communicationClient.SendAsync(new DisposeRemoteContentRequest(_id), CancellationToken.None).Await();
}