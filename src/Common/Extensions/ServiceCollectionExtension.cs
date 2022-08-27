using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace IntelliHome.Common;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddConfigurationManager<TConfiguration>(this IServiceCollection services)
        where TConfiguration : class, IServiceConfiguration =>
        services.
            AddSingleton<IServiceConfiguration, TConfiguration>().
            AddSingleton<IConfigurationManager, ConfigurationManager>();

    public static IServiceCollection AddConfigurationManager(this IServiceCollection services, IServiceConfiguration configuration) =>
        services.
            AddSingleton(configuration).
            AddSingleton<IConfigurationManager, ConfigurationManager>();

    public static IServiceCollection AddEnvironmentDependentSingleton<
        TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TProductionServiceImplementation,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TDevelopmentServiceImplementation>(
        this IServiceCollection serviceCollection,
        IHostEnvironment hostEnvironment)
        where TProductionServiceImplementation : class, TService
        where TService : class
        where TDevelopmentServiceImplementation : class, TService
    {
        Ensure.NotNull(serviceCollection);
        Ensure.NotNull(hostEnvironment);

        return hostEnvironment.IsProduction()
            ? serviceCollection.AddSingleton<TService, TProductionServiceImplementation>()
            : hostEnvironment.IsDevelopment()
                ? serviceCollection.AddSingleton<TService, TDevelopmentServiceImplementation>()
                : throw new Exception($"Invalid environment [{nameof(hostEnvironment.EnvironmentName)}={hostEnvironment.EnvironmentName}]");
    }
}