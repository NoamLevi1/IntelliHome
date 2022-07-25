using IntelliHome.Cli.Configuration;
using IntelliHome.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntelliHome.Cli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ServiceProvider? serviceProvider = null;

        var exitCode =
            await Host.
                CreateDefaultBuilder().
                ConfigureServices(
                    (_, services) =>
                    {
                        services.
                            AddSingleton<IConfigurationManager,CliConfigurationManager>().
                            AddSingleton<HttpClient>().
                            AddSingleton<IDatabase, Database>().
                            AddSingleton<IDockerHelper, DockerHelper>();
                        serviceProvider = services.BuildServiceProvider();
                    }).
                RunCommandLineApplicationAsync<MainCommandGroup>(args);

        if (serviceProvider != null)
        {
            await serviceProvider.DisposeAsync();
        }

        Environment.Exit(exitCode);
    }
}