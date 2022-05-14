using System.Collections.Concurrent;

namespace IntelliHome.Cloud;

public interface IHomeApplianceStore
{
    IEnumerable<Guid> ConnectedHomeApplianceIds { get; }

    void AddOrUpdateHomeAppliance(Guid homeApplianceId, string connectionId);
    string GetConnectionId(Guid homeApplianceId);

    event Action? ConnectedHomeAppliancesChanged;
}

public sealed class HomeApplianceStore : IHomeApplianceStore
{
    private readonly ConcurrentDictionary<Guid, string> _homeApplianceIdToConnectionIdMapping = new();

    public IEnumerable<Guid> ConnectedHomeApplianceIds => _homeApplianceIdToConnectionIdMapping.Keys;

    public void AddOrUpdateHomeAppliance(Guid homeApplianceId, string connectionId)
    {
        _homeApplianceIdToConnectionIdMapping.AddOrUpdate(
            homeApplianceId,
            connectionId,
            (_, _) => connectionId);
        ConnectedHomeAppliancesChanged?.Invoke();
    }

    public string GetConnectionId(Guid homeApplianceId) =>
        _homeApplianceIdToConnectionIdMapping[homeApplianceId];

    public event Action? ConnectedHomeAppliancesChanged;
}