using IntelliHome.Common;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace IntelliHome.Cloud
{
    public sealed class CustomProxyConfigProvider : IProxyConfigProvider
    {
        private readonly string _hostName;
        private readonly IHomeApplianceStore _homeApplianceStore;

        private CancellationTokenSource _cancellationTokenSource = new();

        public CustomProxyConfigProvider(IHomeApplianceStore homeApplianceStore, IConfigurationManager configurationManager)
        {
            _homeApplianceStore = homeApplianceStore;
            _homeApplianceStore.ConnectedHomeAppliancesChanged += HandleHomeAppliancesChanged;

            _hostName = configurationManager.Get<WebApplicationConfiguration>().ServerUrl.Host;
        }

        public IProxyConfig GetConfig() =>
            new HomeApplianceProxyConfig(
                _homeApplianceStore.ConnectedHomeApplianceIds,
                _hostName,
                new CancellationChangeToken(_cancellationTokenSource.Token));

        private void HandleHomeAppliancesChanged()
        {
            var oldCancellationTokenSource = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();

            oldCancellationTokenSource.Cancel();
        }

        private class HomeApplianceProxyConfig : IProxyConfig
        {
            public IReadOnlyList<RouteConfig> Routes { get; }
            public IReadOnlyList<ClusterConfig> Clusters { get; }
            public IChangeToken ChangeToken { get; }

            public HomeApplianceProxyConfig(
                IEnumerable<Guid> homeApplianceIds,
                string hostName,
                IChangeToken changeToken)
            {
                Routes =
                    homeApplianceIds.
                        Select(
                            homeApplianceId =>
                                new RouteConfig
                                {
                                    RouteId = $"{homeApplianceId}-Route",
                                    ClusterId = GetClusterId(homeApplianceId),
                                    Match = new RouteMatch
                                    {
                                        Hosts = new[]
                                        {
                                            $"{homeApplianceId}.{hostName}"
                                        },
                                        Path = "{**catch-all}"
                                    }
                                }).
                        ToList();

                Clusters =
                    homeApplianceIds.
                        Select(
                            homeApplianceId =>
                                new ClusterConfig
                                {
                                    ClusterId = GetClusterId(homeApplianceId),
                                    Destinations = new Dictionary<string, DestinationConfig>
                                    {
                                        ["DefaultDestination"] = new()
                                        {
                                            Address = "http://host.docker.internal:8123"
                                        }
                                    },
                                    Metadata = new Dictionary<string, string>
                                    {
                                        [ForwarderMetadataKey.HomeApplianceId] = homeApplianceId.ToString()
                                    }
                                }).
                        ToList();

                ChangeToken = changeToken;

                string GetClusterId(Guid homeApplianceId) =>
                    $"{homeApplianceId}-Cluster";
            }
        }
    }
}