using IntelliHome.Cloud.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateModel userCreateModel)
    {
        if (!ModelState.IsValid)
        {
            return View(userCreateModel);
        }

        var applicationUser = new ApplicationUser
        {
            UserName = userCreateModel.Name,
            Email = userCreateModel.Email
        };

        var identityResult = await _userManager.CreateAsync(applicationUser, userCreateModel.Password);
        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return View(userCreateModel);
        }

        ViewBag.Message = "User Created Successfully";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] AccountLoginModel accountLoginModel, [FromQuery] string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(accountLoginModel);
        }

        var applicationUser = await _userManager.FindByEmailAsync(accountLoginModel.Email);
        if (applicationUser == null)
        {
            return FailedAuthentication();
        }

        var signInResult =
            await _signInManager.PasswordSignInAsync(
                applicationUser,
                accountLoginModel.Password,
                true,
                false);
        return signInResult.Succeeded
            ? Redirect(returnUrl ?? "/")
            : FailedAuthentication();

        IActionResult FailedAuthentication()
        {
            ModelState.AddModelError(nameof(accountLoginModel.Email), "Login Failed: Invalid Email or Password");
            return View(accountLoginModel);
        }
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}