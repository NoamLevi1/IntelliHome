using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class CommunicationManagerConfiguration : IServiceConfiguration
{
    public ServerConfiguration ServerConfiguration { get; }

    [UsedImplicitly]
    public CommunicationManagerConfiguration(IConfiguration configuration) =>
        ServerConfiguration = new ServerConfiguration(configuration.GetSection(nameof(ServerConfiguration)));
}