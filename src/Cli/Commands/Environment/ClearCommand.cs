using IntelliHome.Cli.Utilities;
using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
public class ClearCommand : CommandBase
{
    private readonly DockerHelper _dockerHelper;

    public ClearCommand(ILoggerFactory loggerFactory, DockerHelper dockerHelper)
        : base(loggerFactory) =>
        _dockerHelper = dockerHelper;

    protected override async Task RunAsync(CancellationToken cancellationToken) =>
        await new[]
        {
            _dockerHelper.RemoveCommunicationManagerContainerAsync(cancellationToken),
            _dockerHelper.RemoveHomeAssistantContainerAsync(cancellationToken)
        }.WhenAllAsync();
}