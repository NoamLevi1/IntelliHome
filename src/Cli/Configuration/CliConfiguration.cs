using IntelliHome.Common;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace IntelliHome.Cli.Configuration;

public sealed class CliConfiguration : IServiceConfiguration
{
    public DatabaseConfiguration DatabaseConfiguration { get; }

    [UsedImplicitly]
    public CliConfiguration(IConfiguration configuration) =>
        DatabaseConfiguration = new DatabaseConfiguration(configuration.GetSection(nameof(DatabaseConfiguration)));

}