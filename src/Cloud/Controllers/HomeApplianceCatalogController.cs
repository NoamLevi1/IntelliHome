using IntelliHome.Cloud.Identity;
using IntelliHome.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace IntelliHome.Cloud.Controllers
{
    [Authorize]
    public sealed class HomeApplianceCatalogController : Controller
    {
        private readonly IDatabase _database;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WebApplicationConfiguration _webApplicationConfiguration;

        public HomeApplianceCatalogController(
            IConfigurationManager configurationManager,
            UserManager<ApplicationUser> userManager,
            IDatabase database)
        {
            _database = database;
            _userManager = userManager;
            _webApplicationConfiguration = configurationManager.Get<WebApplicationConfiguration>();
        }

        public async Task<IActionResult> Index()
        {
            var userId = (await _userManager.GetUserAsync(User))?.Id;
            return View(
                (await (await _database.
                        HomeAppliances.
                        FindAsync(homeAppliance => homeAppliance.OwnerId == userId)).
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
        }

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


        public IActionResult AddHomeAppliance() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHomeAppliance(AddHomeApplianceModel addHomeApplianceModel)
        {
            if (!ModelState.IsValid)
            {
                return View(addHomeApplianceModel);
            }

            var userId = (await _userManager.GetUserAsync(User))?.Id;
            var updateResult = await _database.
                HomeAppliances.
                UpdateOneAsync(
                    homeAppliance =>
                        homeAppliance.Id == addHomeApplianceModel.Id &&
                        homeAppliance.OwnerId == null,
                    new UpdateDefinitionBuilder<HomeAppliance>().Set(homeAppliance => homeAppliance.OwnerId, userId));


            if (updateResult.MatchedCount == 0)
            {
                ModelState.AddModelError("UnsupportedHomeAppliance", "Home appliance is either already assigned or does not exist");
                return View(addHomeApplianceModel);
            }

            return RedirectToAction("Index");
        }
    }
}