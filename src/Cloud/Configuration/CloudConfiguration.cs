using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.Cloud;

public sealed class CloudConfiguration : CommonConfiguration
{
    public DatabaseConfiguration DatabaseConfiguration { get; }

    [UsedImplicitly]
    public CloudConfiguration(IConfiguration configuration)
        : base(configuration) =>
        DatabaseConfiguration = new DatabaseConfiguration(configuration.GetSection(nameof(DatabaseConfiguration)));
}