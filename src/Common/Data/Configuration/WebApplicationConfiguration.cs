using Microsoft.Extensions.Configuration;

namespace IntelliHome.Common;

public sealed class WebApplicationConfiguration : IServiceConfiguration
{
    public Uri ServerUrl { get; }

    public WebApplicationConfiguration(IConfiguration configuration) =>
        ServerUrl = new Uri(configuration.GetValue<string>(nameof(ServerUrl)));
}