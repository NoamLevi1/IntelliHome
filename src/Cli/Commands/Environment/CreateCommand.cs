﻿using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
public class CreateCommand : CommandBase
{
    private readonly IDockerHelper _dockerHelper;

    [Option("--overwrite")]
    public bool Overwrite { get; set; }
    [Option("--verbose")]
    public bool Verbose { get; set; }

    public CreateCommand(
        ILoggerFactory loggerFactory,
        CommandLineApplication commandLineApplication,
        IDockerHelper dockerHelper)
        : base(loggerFactory, commandLineApplication) =>
        _dockerHelper = dockerHelper;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(RunAsync)} started");

        await new[]
        {
            _dockerHelper.CreateCommunicationManagerContainerAsync(
                Verbose,
                Overwrite,
                cancellationToken),
            _dockerHelper.CreateHomeAssistantContainerAsync(
                Verbose,
                Overwrite,
                cancellationToken)
        }.WhenAllAsync();

        Logger.LogInformation($"{nameof(RunAsync)} finished");
    }
}