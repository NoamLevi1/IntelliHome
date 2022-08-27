using IntelliHome.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        return services.
            AddScoped(_ => new ExtendedCookieAuthenticationEvents(host)).
            ConfigureApplicationCookie(
            options =>
            {
                options.EventsType = typeof(ExtendedCookieAuthenticationEvents);
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

    private class ExtendedCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly string _host;

        public ExtendedCookieAuthenticationEvents(string host) => 
            _host = host;
        
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            var redirectUri = new Uri(context.RedirectUri);

            var redirectPathAndQuery = Uri.UnescapeDataString(redirectUri.Query.Replace("?ReturnUrl=", string.Empty)).Split("?");
            var returnUrlBuilder = new UriBuilder
            {
                Host = redirectUri.Host,
                Scheme = redirectUri.Scheme,
                Path = redirectPathAndQuery[0],
                Port = redirectUri.Port
            };
            if (redirectPathAndQuery.Length > 1)
            {
                returnUrlBuilder.Query = redirectPathAndQuery[1];
            }
            
            var returnUrl = Uri.EscapeDataString(returnUrlBuilder.ToString());
            var uri = new UriBuilder
            {
                Scheme = context.Request.Scheme,
                Host = $"portal.{_host}",
                Port = context.Request.Host.Port ?? -1,
                Path = redirectUri.AbsolutePath,
                Query = $"ReturnUrl={returnUrl}"
            }.Uri;
            
            context.Response.Redirect(uri.ToString());
            return Task.CompletedTask;
        }
    }
}