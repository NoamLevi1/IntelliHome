using IntelliHome.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace IntelliHome.Cloud.Controllers
{
    [Authorize]
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
                            device.Id,
                            number + 1,
                            device.Name ?? "Unconfigured",
                            device.IsConnected,
                            uri);
                    }));

        public async Task<IActionResult> EditHomeAppliance(Guid id)
        {
            var homeAppliance = 
                await (await _database.
                    HomeAppliances.
                    FindAsync(homeAppliance => homeAppliance.Id == id)).
                    SingleOrDefaultAsync();

            if (homeAppliance is null)
            {
                return NotFound();
            }

            var editHomeApplianceModel = new EditHomeApplianceModel
            {
                Id = homeAppliance.Id,
                Name = homeAppliance.Name
            };
            return View(editHomeApplianceModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditHomeAppliance([FromForm] EditHomeApplianceModel editHomeApplianceModel)
        {
            await _database.
                HomeAppliances.
                UpdateOneAsync(
                    homeAppliance => homeAppliance.Id == editHomeApplianceModel.Id,
                    new UpdateDefinitionBuilder<HomeAppliance>().Set(homeAppliance => homeAppliance.Name, editHomeApplianceModel.Name));

            return RedirectToAction("Index");
        }
    }
}