using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.Cloud;

public sealed class CloudConfiguration : IServiceConfiguration
{
    [UsedImplicitly]
    public CloudConfiguration(IConfiguration configuration)
    {
    }
}