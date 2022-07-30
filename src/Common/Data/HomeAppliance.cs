using MongoDB.Bson.Serialization.Attributes;

namespace IntelliHome.Common;

public sealed class HomeAppliance
{
    [BsonId]
    public Guid Id { get; }

    public string? Name { get; set; }
    public Guid OwnerId => Guid.Empty;
    public string? ConnectionId { get; set; }

    public bool IsConnected => ConnectionId != null;

    [BsonConstructor]
    public HomeAppliance(Guid id) =>
        Id = id;
}