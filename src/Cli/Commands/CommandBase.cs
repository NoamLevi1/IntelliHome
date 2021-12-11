using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[HelpOption("--help")]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public abstract class CommandBase
{
    protected ILogger Logger { get; }
    protected CommandLineApplication CommandLineApplication { get; }

    protected virtual bool IsRequireAdminPrivileges => false;

    protected CommandBase(ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        CommandLineApplication = commandLineApplication;
    }

    protected abstract Task RunAsync(CancellationToken cancellationToken);

    public async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (IsRequireAdminPrivileges && !ProcessAuthorizationHelper.IsRunningWithAdminPrivileges())
            {
                throw new Exception("Command must run with admin privileges");
            }

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