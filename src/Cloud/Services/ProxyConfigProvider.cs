using IntelliHome.Common;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using Yarp.ReverseProxy.Configuration;

namespace IntelliHome.Cloud;

public sealed class CustomProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly ILogger<CustomProxyConfigProvider> _logger;
    private readonly IDatabase _database;
    private readonly string _hostName;
    private readonly CancellationTokenSource _watcherCancellationTokenSource = new();

    private CancellationTokenSource _cancellationTokenSource = new();

    public CustomProxyConfigProvider(ILogger<CustomProxyConfigProvider> logger, IDatabase database, IConfigurationManager configurationManager)
    {
        _logger = logger;
        _database = database;

        _hostName = configurationManager.Get<WebApplicationConfiguration>().ServerUrl.Host;

        Task.Run(() => WatchDatabase(_watcherCancellationTokenSource.Token));
    }

    public IProxyConfig GetConfig() =>
        new HomeApplianceProxyConfig(
            _database.
                HomeAppliances.
                Find(homeAppliance => homeAppliance.ConnectionId != null).
                Project(homeAppliance => homeAppliance.Id).
                ToList(),
            _hostName,
            new CancellationChangeToken(_cancellationTokenSource.Token));

    public void Dispose()
    {
        _watcherCancellationTokenSource.Cancel();
    }

    private void HandleHomeAppliancesConnectionChanged()
    {
        var oldCancellationTokenSource = _cancellationTokenSource;
        _cancellationTokenSource = new CancellationTokenSource();

        oldCancellationTokenSource.Cancel();
    }

    private async Task WatchDatabase(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var stream = await _database.HomeAppliances.WatchAsync(cancellationToken: cancellationToken);
                await stream.ForEachAsync(
                    document =>
                    {
                        if (document.UpdateDescription?.UpdatedFields.Contains(nameof(HomeAppliance.ConnectionId)) ?? true)
                        {
                            HandleHomeAppliancesConnectionChanged();
                        }
                    },
                    cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"{nameof(WatchDatabase)} caught exception [{nameof(exception)}={exception}]");
            }
        }
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