using Microsoft.Extensions.Configuration;

namespace IntelliHome.Common;

public abstract class CommonConfiguration : IServiceConfiguration
{
    public WebApplicationConfiguration WebApplicationConfiguration { get; }

    protected CommonConfiguration(IConfiguration configuration) =>
        WebApplicationConfiguration = new WebApplicationConfiguration(configuration.GetSection(nameof(WebApplicationConfiguration)));
}