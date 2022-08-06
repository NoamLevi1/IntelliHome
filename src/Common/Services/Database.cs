using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace IntelliHome.Common;

public interface IDatabase
{
    IMongoCollection<HomeAppliance> HomeAppliances { get; }
}

public sealed class Database : IDatabase
{
    public const string DatabaseName = nameof(IntelliHome);

    public IMongoCollection<HomeAppliance> HomeAppliances { get; }

    public Database(IConfigurationManager configurationManager)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var configuration = configurationManager.Get<DatabaseConfiguration>();

        var mongoClient = new MongoClient(configuration.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(DatabaseName);

        HomeAppliances = mongoDatabase.GetCollection<HomeAppliance>(nameof(HomeAppliances));
    }
}

public sealed class DatabaseConfiguration : IServiceConfiguration
{
    public string ConnectionString { get; }

    public DatabaseConfiguration(IConfiguration configuration) =>
        ConnectionString = configuration.GetValue<string>(nameof(ConnectionString));
}