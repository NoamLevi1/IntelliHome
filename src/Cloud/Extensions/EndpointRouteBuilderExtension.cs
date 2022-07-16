using IntelliHome.Common;

namespace IntelliHome.Cloud.Extensions;

public static class EndpointRouteBuilderExtension
{
    public static void MapControllerRouteWithHostNamePrefix(
        this IEndpointRouteBuilder builder,
        string name,
        string pattern,
        string prefix)
    {
        Ensure.NotNull(builder);
        Ensure.NotNullOrWhiteSpace(name);
        Ensure.NotNullOrWhiteSpace(pattern);
        Ensure.NotNullOrWhiteSpace(prefix);

        builder.MapControllerRoute(
            name,
            pattern,
            constraints: new
            {
                customConstraint = new HostNamePrefixRouteConstraint(prefix)
            });
    }

    private class HostNamePrefixRouteConstraint : IRouteConstraint
    {
        private readonly string _prefix;

        public HostNamePrefixRouteConstraint(string prefix)
        {
            Ensure.NotNullOrWhiteSpace(prefix);

            _prefix = prefix;
        }

        public bool Match(
            HttpContext? httpContext,
            IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection) =>
            httpContext?.
                Request.
                Host.
                Host.
                StartsWith(_prefix, StringComparison.OrdinalIgnoreCase) ??
            false;
    }
}