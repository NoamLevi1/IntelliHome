using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace IntelliHome.Common;

public interface IDatabase
{
    IMongoCollection<HomeAppliance> HomeAppliances { get; }
}

public sealed class Database : IDatabase
{
    private const string _databaseName = nameof(IntelliHome);

    public IMongoCollection<HomeAppliance> HomeAppliances { get; }

    public Database(IConfigurationManager configurationManager)
    {
        var configuration = configurationManager.Get<DatabaseConfiguration>();

        var mongoClient = new MongoClient(configuration.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(_databaseName);

        HomeAppliances = mongoDatabase.GetCollection<HomeAppliance>(nameof(HomeAppliances));
    }
}

public sealed class DatabaseConfiguration : IServiceConfiguration
{
    public string ConnectionString { get; }

    public DatabaseConfiguration(IConfiguration configuration)
    {
        ConnectionString = configuration.GetValue<string>(nameof(ConnectionString));
    }

    public DatabaseConfiguration(string connectionString)
    {
        Ensure.NotNullOrWhiteSpace(connectionString);
        
        ConnectionString = connectionString;
    }
}