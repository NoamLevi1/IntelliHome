namespace IntelliHome.Common;

public interface IConfigurationManager
{
    TConfiguration Get<TConfiguration>()
        where TConfiguration : IServiceConfiguration;
}

public sealed class ConfigurationManager : IConfigurationManager
{
    private readonly Dictionary<Type, object> _configurationTypeToConfigurationInstanceMapping;

    public ConfigurationManager(IServiceConfiguration serviceConfiguration) =>
        _configurationTypeToConfigurationInstanceMapping =
            serviceConfiguration.
                GetType().
                GetProperties().
                Where(property => typeof(IServiceConfiguration).IsAssignableFrom(property.PropertyType)).
                ToDictionary(
                    property => property.PropertyType,
                    property =>
                        property.GetValue(serviceConfiguration) ??
                        throw new Exception($"Found unexpected null configuration [configurationType={property.PropertyType.Name}]"));

    public TConfiguration Get<TConfiguration>()
        where TConfiguration : IServiceConfiguration =>
        (TConfiguration) _configurationTypeToConfigurationInstanceMapping[typeof(TConfiguration)];
}

public interface IServiceConfiguration
{
}