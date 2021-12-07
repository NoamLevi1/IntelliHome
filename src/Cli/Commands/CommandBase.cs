using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[HelpOption("--help")]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public abstract class CommandBase
{
    protected ILogger Logger { get; }
    protected CommandLineApplication CommandLineApplication { get; private set; } = null!;

    protected CommandBase(ILoggerFactory loggerFactory) =>
        Logger = loggerFactory.CreateLogger(GetType());

    protected abstract Task RunAsync(CancellationToken cancellationToken);

    public async Task<int> OnExecuteAsync(CommandLineApplication commandLineApplication, CancellationToken cancellationToken)
    {
        CommandLineApplication = commandLineApplication;

        try
        {
            await RunAsync(cancellationToken);
            return 0;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "MainCommandGroup caught exceptions");
            return 1;
        }
    }
}