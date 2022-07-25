using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IntelliHome.Common;

public sealed class HomeAppliance
{
    [BsonId]
    public Guid Id { get; }

    public string? Name { get; set; }
    public Guid OwnerId => Guid.Empty;
    public bool IsConnected { get; set; }

    public HomeAppliance(Guid id) =>
        Id = id;
}