using System.Diagnostics.CodeAnalysis;
using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public static class ServiceCollectionExtension
{
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