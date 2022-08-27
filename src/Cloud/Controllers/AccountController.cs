using IntelliHome.Cloud.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud.Controllers;

public sealed class AccountController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IEmailSender emailSender,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _emailSender = emailSender;
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

    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(AccountForgotPasswordModel accountForgotPasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return View(accountForgotPasswordModel);
        }

        var user = await _userManager.FindByEmailAsync(accountForgotPasswordModel.Email);
        if (user is null)
        {
            ModelState.AddModelError("UnableToFindUser", "Unable to find user");
            return View(accountForgotPasswordModel);
        }

        var passwordResetUrlCallback =
            Url.Action(
                "ResetPassword",
                "Account",
                new
                {
                    Token = await _userManager.GeneratePasswordResetTokenAsync(user),
                    user.Email
                },
                Request.Scheme);
        if (passwordResetUrlCallback is null)
        {
            ModelState.AddModelError("UnableToBuildCallback", "Unable to build request callback. please try again.");
            return View(accountForgotPasswordModel);
        }

        await _emailSender.SendAsync(
            new EmailMessage(
                user.Email,
                "IntelliHome Password Reset",
                passwordResetUrlCallback));

        ViewBag.Message = "Message Sent! Please check your email to reset your password.";
        return View(accountForgotPasswordModel);
    }

    [AllowAnonymous]
    public IActionResult ResetPassword(string token, string email) =>
        View(
            new AccountResetPasswordModel
            {
                Token = token,
                Email = email
            });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(AccountResetPasswordModel accountResetPasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return View(accountResetPasswordModel);
        }

        var user = await _userManager.FindByEmailAsync(accountResetPasswordModel.Email);
        if (user is null)
        {
            ModelState.AddModelError("UnableToFindUser", "Unable to find user.");
            return View(accountResetPasswordModel);
        }

        var resetPassResult =
            await _userManager.ResetPasswordAsync(
                user,
                accountResetPasswordModel.Token,
                accountResetPasswordModel.Password);
        if (!resetPassResult.Succeeded)
        {
            foreach (var error in resetPassResult.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return View(accountResetPasswordModel);
        }

        ViewBag.Message = "Password was reset! you can now login with your new password!";
        return View(accountResetPasswordModel);
    }
}