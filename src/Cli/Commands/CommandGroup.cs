using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

public class CommandGroup : CommandBase
{
    public CommandGroup(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
    }

    protected sealed override Task RunAsync(CancellationToken cancellationToken)
    {
        CommandLineApplication.ShowHelp();
        return Task.CompletedTask;
    }
}