using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
[Subcommand(typeof(EnvironmentCommandGroup))]
public sealed class MainCommandGroup : CommandGroup
{
    public MainCommandGroup(ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
    }
}