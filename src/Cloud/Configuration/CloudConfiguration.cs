using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.Cloud;

public sealed class CloudConfiguration : CommonConfiguration
{
    [UsedImplicitly]
    public CloudConfiguration(IConfiguration configuration)
        : base(configuration)
    {
    }
}