using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IntelliHome.Cloud;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(
            new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
}