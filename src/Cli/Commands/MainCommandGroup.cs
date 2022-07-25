using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
[Subcommand(typeof(EnvironmentCommandGroup),typeof(HomeApplianceCommandGroup))]
public sealed class MainCommandGroup : CommandGroup
{
    [Option]
    public EnvironmentType EnvironmentType { get; set; }

    public MainCommandGroup(IConfigurationManager configurationManager,ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
        ((CliConfigurationManager) configurationManager).EnvironmentType = EnvironmentType;
    }
}