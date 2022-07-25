using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud.Controllers
{
    public sealed class HomeApplianceCatalogController : Controller
    {
        static readonly IList<HomeAppliance> devicesList =
            new List<HomeAppliance>()
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
                                device.IsConnected ?? false,
                                uri,
                                device.Id);
                        }));

        public IActionResult EditHomeAppliance(Guid id)
        {
            //get the HomeAppliance specified by the ID

            // testing
            var editingHomeAppliance = devicesList.FirstOrDefault(s => s.Id == id);

            if (editingHomeAppliance == null)
            {
                return NotFound();
            }
            
            var edithomeApplianceModel = new EditHomeApplianceModel()
            {
                Id = editingHomeAppliance.Id,
                Name = editingHomeAppliance.Name
            };
            return View(edithomeApplianceModel);
        }

        [HttpPost]
        public ActionResult EditHomeAppliance([FromForm] EditHomeApplianceModel homeAppliance)
        {
            Ensure.NotNull(devicesList);
            //update HomeAppliance In the DB

            var appliance = new HomeAppliance(homeAppliance.Id)
            {
                Name = homeAppliance.Name
            };

            var editingHomeAppliance = devicesList.FirstOrDefault(s => s.Id == appliance.Id);
            if (editingHomeAppliance == null)
            {
                return NotFound();
            }
            editingHomeAppliance.Aggregate(appliance);





            //HomeAppliance oldHomeAppliance = devicesList?.Where(s => s.Id == homeAppliance.Id).FirstOrDefault() ?? new HomeAppliance(Guid.NewGuid());
            devicesList.Remove(editingHomeAppliance);
            devicesList.Add(editingHomeAppliance);
            return RedirectToAction("Index");
        }
    }
}