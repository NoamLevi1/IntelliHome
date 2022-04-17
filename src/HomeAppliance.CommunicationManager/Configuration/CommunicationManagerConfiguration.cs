using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class CommunicationManagerConfiguration : IServiceConfiguration
{
    [UsedImplicitly]
    public CommunicationManagerConfiguration(IConfiguration configuration)
    {
    }
}