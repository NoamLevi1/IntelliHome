using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace IntelliHome.Cloud.Controllers
{
    public sealed class HomeApplianceCatalogController : Controller
    {
        private readonly IDatabase _database;
        private readonly WebApplicationConfiguration _webApplicationConfiguration;

        public HomeApplianceCatalogController(IConfigurationManager configurationManager, IDatabase database)
        {
            _database = database;
            _webApplicationConfiguration = configurationManager.Get<WebApplicationConfiguration>();
        }

        public async Task<IActionResult> Index() =>
            View(
                (await (await _database.
                        HomeAppliances.
                        FindAsync(_ => true)).
                    ToListAsync()).
                OrderByDescending(_ => _.IsConnected).
                Select(
                    (device, number) =>
                    {
                        var uri = new UriBuilder
                        {
                            Scheme = _webApplicationConfiguration.ServerUrl.Scheme,
                            Port = _webApplicationConfiguration.ServerUrl.Port,
                            Host = $"{device.Id}.{_webApplicationConfiguration.ServerUrl.Host}"
                        }.Uri;

                        return new HomeApplianceCatalogModel(
                            number + 1,
                            device.Name ?? "Unconfigured",
                            device.IsConnected,
                            uri);
                    }));
    }
}