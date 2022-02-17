using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace IntelliHome.Cloud
{
    public sealed class CustomProxyConfigProvider : IProxyConfigProvider
    {
        private const string _defaultClusterId = "DefaultCluster";

        public IProxyConfig GetConfig() =>
            new CustomMemoryConfig
            {
                Routes = new[]
                {
                    new RouteConfig
                    {
                        RouteId = "DefaultRoute",
                        ClusterId = _defaultClusterId,
                        Match = new RouteMatch
                        {
                            Path = "{**catch-all}"
                        }
                    }
                },
                Clusters = new[]
                {
                    new ClusterConfig
                    {
                        ClusterId = _defaultClusterId,
                        Destinations = new Dictionary<string, DestinationConfig>
                        {
                            ["DefaultDestination"] = new()
                            {
                                Address = "http://host.docker.internal:8123"
                            }
                        }
                    }
                }
            };

        private class CustomMemoryConfig : IProxyConfig
        {
            private static readonly CancellationTokenSource _cancellationTokenSource = new();

            public IReadOnlyList<RouteConfig> Routes { get; init; } = Array.Empty<RouteConfig>();

            public IReadOnlyList<ClusterConfig> Clusters { get; init; } = Array.Empty<ClusterConfig>();

            public IChangeToken ChangeToken { get; } = new CancellationChangeToken(_cancellationTokenSource.Token);
        }
    }
}