using IntelliHome.Common;
using Microsoft.AspNetCore.Authorization;

namespace IntelliHome.Cloud.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHomeApplianceOwnerAuthorizationRequirement(this IServiceCollection services)
    {
        Ensure.NotNull(services);

        return services.
            AddSingleton<HomeApplianceOwnerAuthorizationRequirement>().
            AddAuthorization(
            (options, serviceProvider) =>
                options.AddPolicy(
                    nameof(HomeApplianceOwnerAuthorizationRequirement),
                    policyBuilder =>
                        policyBuilder.
                            Requirements.
                            Add(serviceProvider.GetRequiredService<HomeApplianceOwnerAuthorizationRequirement>())));
    }

    public static IServiceCollection ConfigureCrossSubDomainsApplicationCookie(this IServiceCollection services, string host)
    {
        Ensure.NotNull(services);
        Ensure.NotNullOrWhiteSpace(host);

        return services.ConfigureApplicationCookie(
            options =>
            {
                options.Cookie.Domain = $".{host}";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
    }

    private static IServiceCollection AddAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions, IServiceProvider> configure)
    {
        Ensure.NotNull(services);
        Ensure.NotNull(configure);

        services.AddOptions<AuthorizationOptions>().Configure(configure);
        return services.AddAuthorization();
    }
}