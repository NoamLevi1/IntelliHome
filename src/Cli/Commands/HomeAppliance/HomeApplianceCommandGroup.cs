using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command("home-appliance")]
[Subcommand(typeof(AddCommand),typeof(RemoveCommand))]

public class HomeApplianceCommandGroup: CommandGroup
{
    public HomeApplianceCommandGroup( ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
    }
}