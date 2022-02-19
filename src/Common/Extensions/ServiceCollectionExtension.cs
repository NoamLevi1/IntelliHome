using Microsoft.Extensions.DependencyInjection;

namespace IntelliHome.Common;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddConfigurationManager<TConfiguration>(this IServiceCollection services)
        where TConfiguration : class, IServiceConfiguration =>
        services.
            AddSingleton<IServiceConfiguration, TConfiguration>().
            AddSingleton<IConfigurationManager, ConfigurationManager>();
}