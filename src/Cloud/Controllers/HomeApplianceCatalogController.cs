using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud.Controllers
{
    public sealed class HomeApplianceCatalogController : Controller
    {
        static ICollection<HomeAppliance> devicesList =
            new[]
            {
                new HomeAppliance(Guid.NewGuid()) {Name = "Living Room", IsConnected = true},
                new HomeAppliance(Guid.NewGuid()) {Name = "Dining Room", IsConnected = false},
                new HomeAppliance(Guid.NewGuid()) {Name = "Sport Room", IsConnected = false},
                new HomeAppliance(Guid.NewGuid()) {Name = "Beds Room", IsConnected = true},
                new HomeAppliance(Guid.NewGuid()) {Name = "Children Room", IsConnected = true},
                new HomeAppliance(Guid.NewGuid()) {Name = "Dog's Room", IsConnected = true}
            };
        private readonly WebApplicationConfiguration _webApplicationConfiguration;

        public HomeApplianceCatalogController(IConfigurationManager configurationManager) =>
            _webApplicationConfiguration = configurationManager.Get<WebApplicationConfiguration>();

        public IActionResult Index() =>
            View(
                devicesList.
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
                                uri,device.Id);
                        }));

        public IActionResult EditHomeAppliance(Guid id)
        {
            //get the HomeAppliance specified by the ID

            // testing
            HomeAppliance editingHomeAppliance = devicesList.Where(s => s.Id == id).FirstOrDefault();

            return View(editingHomeAppliance);

        }

        [HttpPost]
        public ActionResult EditHomeAppliance([Bind(include:"ID,Name,IsConnected")] HomeAppliance homeAppliance)
        {
            Ensure.NotNull(devicesList);
            //update HomeAppliance In the DB

            HomeAppliance oldHomeAppliance = devicesList?.Where(s => s.Id == homeAppliance.Id).FirstOrDefault() ?? new HomeAppliance(Guid.NewGuid());
            devicesList.Remove(oldHomeAppliance);
            devicesList.Add(homeAppliance);
            return RedirectToAction("Index");
        }
    }
}