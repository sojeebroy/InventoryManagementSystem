using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Inventory_Management_System.Models;

namespace Inventory_Management_System.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Login page with social login options
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Initiate external (OAuth) login
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult LoginWithProvider(string provider, string? returnUrl = null)
    {
        // Redirect to external provider for authentication
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Callback from external provider (Google/Facebook)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ModelState.AddModelError(string.Empty, "Error loading external login information.");
            return RedirectToAction(nameof(Login));
        }

        // Get provider name and user ID
        var provider = info.LoginProvider;
        var providerUserId = info.ProviderKey;

        // Try to sign in existing user
        var result = await _signInManager.ExternalLoginSignInAsync(provider, providerUserId, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);
            if (user != null)
            {
                // Check if user is blocked
                if (user.IsBlocked)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("BlockedUser");
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserId} logged in via {Provider}", user.Id, provider);
            }

            return LocalRedirect(returnUrl);
        }

        // If we get here, new user - need to create account
        if (result.IsNotAllowed || result.IsLockedOut)
        {
            return RedirectToAction(nameof(Login));
        }

        // Extract user info from provider
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
        var pictureUrl = info.Principal.FindFirstValue("picture");

        // Create new user
        var newUser = new ApplicationUser
        {
            Email = email ?? $"{provider}_{providerUserId}@oauth.local",
            UserName = email?.Split('@')[0] ?? $"{provider}_{providerUserId}",
            FirstName = givenName ?? name?.Split(' ').FirstOrDefault() ?? "User",
            LastName = surname ?? name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
            Provider = provider,
            ProviderUserId = providerUserId,
            ProfilePictureUrl = pictureUrl,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Ensure unique email
        int counter = 1;
        var originalEmail = newUser.Email;
        while (await _userManager.FindByEmailAsync(newUser.Email) != null)
        {
            newUser.Email = $"{originalEmail.Split('@')[0]}_{counter}@oauth.local";
            newUser.UserName = newUser.Email.Split('@')[0];
            counter++;
        }

        // Create user without password (OAuth users don't have passwords)
        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(Login));
        }

        // Add external login
        var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
        if (!addLoginResult.Succeeded)
        {
            foreach (var error in addLoginResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(Login));
        }

        // Assign User role by default
        await _userManager.AddToRoleAsync(newUser, "User");

        // Sign in new user
        await _signInManager.SignInAsync(newUser, isPersistent: false);

        _logger.LogInformation("New user {Email} created and signed in via {Provider}", newUser.Email, provider);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Display blocked user message
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult BlockedUser()
    {
        return View();
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out.", User.FindFirstValue(ClaimTypes.NameIdentifier));
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Logout confirmation GET (for links)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> LogoutConfirm()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
