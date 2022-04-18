using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class ServerConfiguration : IServiceConfiguration
{
    public Uri ServerUrl { get; }

    public ServerConfiguration(IConfiguration configuration) => 
        ServerUrl = new Uri(configuration.GetValue<string>(nameof(ServerUrl)));
}