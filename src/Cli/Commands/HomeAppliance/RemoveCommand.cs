using IntelliHome.Common;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace IntelliHome.Cli;

[Command]
public class RemoveCommand : CommandBase
{
    private readonly IDatabase _database;

    [Option("--id")]
    public Guid Id { get; set; }

    public RemoveCommand(IDatabase database, ILoggerFactory loggerFactory, CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
        _database = database;
    }

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await _database.
            HomeAppliances.
            DeleteOneAsync(
                homeAppliance => homeAppliance.Id == Id,
                cancellationToken);
    }
}