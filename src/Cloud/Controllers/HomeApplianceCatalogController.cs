using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud.Controllers
{
    public sealed class HomeApplianceCatalogController : Controller
    {
        static readonly IReadOnlyCollection<HomeAppliance> _devicesList =
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
                _devicesList.
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