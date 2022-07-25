using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace IntelliHome.Cli;

[Command]
public class AddCommand : CommandBase
{
    private readonly IDatabase _database;

    [Option("--id")]
    public Guid Id { get; set; }
    [Option("--name")]
    public string? Name { get; set; }

    public AddCommand(
        IDatabase database,
        ILoggerFactory loggerFactory,
        CommandLineApplication commandLineApplication)
        : base(loggerFactory, commandLineApplication)
    {
        _database = database;
    }

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await _database.
            HomeAppliances.
            InsertOneAsync(
                new Common.HomeAppliance(Id) {Name = Name},
                cancellationToken: cancellationToken);
    }
}