using IntelliHome.Common;

namespace IntelliHome.Cloud;

public interface IClientStore
{
    public ConcurrentHashSet<string> Clients { get; }
}

public sealed class ClientStore : IClientStore
{
    public ConcurrentHashSet<string> Clients { get; } = new();
}