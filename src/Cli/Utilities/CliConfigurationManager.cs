using IntelliHome.Common;

namespace IntelliHome.Cli;

public class CliConfigurationManager : IConfigurationManager
{
    private Dictionary<EnvironmentType, Dictionary<Type, IServiceConfiguration>> _configurationTypeToConfiguration;

    public EnvironmentType EnvironmentType { get; set; }

    public CliConfigurationManager()
    {
        _configurationTypeToConfiguration =
            new()
            {
                [EnvironmentType.Development] = new Dictionary<Type, IServiceConfiguration>
                {
                    [typeof(DatabaseConfiguration)] = new DatabaseConfiguration("mongodb://localhost:27017")
                },
                [EnvironmentType.Production] = new Dictionary<Type, IServiceConfiguration>
                {
                    [typeof(DatabaseConfiguration)] = new DatabaseConfiguration("mongodb://localhost:27017")
                }
            };
    }

    public TConfiguration Get<TConfiguration>()
        where TConfiguration : IServiceConfiguration =>
        (TConfiguration) _configurationTypeToConfiguration[EnvironmentType][typeof(TConfiguration)];
}

public enum EnvironmentType
{
    Development,
    Production
}