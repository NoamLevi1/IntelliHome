using System.Collections.Concurrent;
using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IRemoteContentManager
{
    Guid AddContent(HttpContent httpContent);
    Task<CreateContentReadStreamRemoteContentResponse> CreateContentReadStreamAsync(CreateContentReadStreamRemoteContentRequest request);
    void Dispose(DisposeRemoteContentRequest request);
}

public sealed class RemoteContentManager : IRemoteContentManager
{
    private readonly IRemoteStreamManager _remoteStreamManager;

    private readonly ConcurrentDictionary<Guid, HttpContent> _contentIdToContentMapping;

    public RemoteContentManager(IRemoteStreamManager remoteStreamManager)
    {
        _remoteStreamManager = remoteStreamManager;

        _contentIdToContentMapping = new ConcurrentDictionary<Guid, HttpContent>();
    }

    public Guid AddContent(HttpContent httpContent)
    {
        var id = Guid.NewGuid();

        if (!_contentIdToContentMapping.TryAdd(id, httpContent))
        {
            throw new Exception($"Content id already exist [{nameof(id)}={id}]");
        }

        return id;
    }

    public async Task<CreateContentReadStreamRemoteContentResponse> CreateContentReadStreamAsync(CreateContentReadStreamRemoteContentRequest request)
    {
        var stream = await GetContent(request.ContentId).ReadAsStreamAsync();

        var streamId = _remoteStreamManager.AddStream(stream);

        return new CreateContentReadStreamRemoteContentResponse(streamId);
    }

    public void Dispose(DisposeRemoteContentRequest request) =>
        GetContent(request.ContentId).Dispose();

    private HttpContent GetContent(Guid id) =>
        _contentIdToContentMapping.GetValueOrDefault(id) ?? throw new Exception($"Unable to find content [{nameof(id)}={id}]");
}