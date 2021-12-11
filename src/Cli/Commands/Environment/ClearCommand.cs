using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
public class ClearCommand : CommandBase
{
    private readonly DockerHelper _dockerHelper;

    public ClearCommand(
        ILoggerFactory loggerFactory,
        CommandLineApplication commandLineApplication,
        DockerHelper dockerHelper)
        : base(loggerFactory, commandLineApplication) =>
        _dockerHelper = dockerHelper;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(RunAsync)} started");

        await new[]
        {
            _dockerHelper.RemoveCommunicationManagerContainerAsync(cancellationToken),
            _dockerHelper.RemoveHomeAssistantContainerAsync(cancellationToken)
        }.WhenAllAsync();

        Logger.LogInformation($"{nameof(RunAsync)} finished");
    }
}