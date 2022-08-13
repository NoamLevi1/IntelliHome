using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using IntelliHome.Cloud.Identity;
using Microsoft.AspNetCore.Identity;

namespace IntelliHome.Cloud;

public sealed class HomeController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public HomeController(SignInManager<ApplicationUser> signInManager) => _signInManager = signInManager;

    public IActionResult Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "HomeApplianceCatalog");
        }

        return View();
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(
            new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
}