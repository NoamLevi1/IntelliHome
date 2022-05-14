using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public sealed class CommunicationManagerConfiguration : CommonConfiguration
{
    [UsedImplicitly]
    public CommunicationManagerConfiguration(IConfiguration configuration)
        : base(configuration)
    {
    }
}