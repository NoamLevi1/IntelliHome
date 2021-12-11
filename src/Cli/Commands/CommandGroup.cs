using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

public class CommandGroup : CommandBase
{
    public CommandGroup(ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
    }

    protected sealed override Task RunAsync(CancellationToken cancellationToken)
    {
        CommandLineApplication.ShowHelp();
        return Task.CompletedTask;
    }
}