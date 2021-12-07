using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command("environment", "env")]
[Subcommand(
    typeof(InstallDependenciesCommand),
    typeof(CreateCommand),
    typeof(ClearCommand))]
public class EnvironmentCommandGroup : CommandGroup
{
    public EnvironmentCommandGroup(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
    }
}