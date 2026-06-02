using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.ViewModels;
using Inventory_Management_System.Services.Interfaces;

namespace Inventory_Management_System.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IInventoryService _inventoryService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IInventoryService inventoryService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult LoginWithProvider(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

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

        var provider = info.LoginProvider;
        var providerUserId = info.ProviderKey;

        var result = await _signInManager.ExternalLoginSignInAsync(provider, providerUserId, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);
            if (user != null)
            {
                if (user.IsBlocked)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("BlockedUser");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserId} logged in via {Provider}", user.Id, provider);
            }

            return LocalRedirect(returnUrl);
        }

        if (result.IsNotAllowed || result.IsLockedOut)
        {
            return RedirectToAction(nameof(Login));
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
        var pictureUrl = info.Principal.FindFirstValue("picture");

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

        int counter = 1;
        var originalEmail = newUser.Email;
        while (await _userManager.FindByEmailAsync(newUser.Email) != null)
        {
            newUser.Email = $"{originalEmail.Split('@')[0]}_{counter}@oauth.local";
            newUser.UserName = newUser.Email.Split('@')[0];
            counter++;
        }

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(Login));
        }

        var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
        if (!addLoginResult.Succeeded)
        {
            foreach (var error in addLoginResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(Login));
        }

        await _userManager.AddToRoleAsync(newUser, "User");
        await _signInManager.SignInAsync(newUser, isPersistent: false);

        _logger.LogInformation("New user {Email} created and signed in via {Provider}", newUser.Email, provider);

        return LocalRedirect(returnUrl);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(string? id, int ownedPage = 1, int ownedPageSize = 10)
    {
        if (string.IsNullOrEmpty(id))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            id = currentUserId;
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new UserProfileViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Provider = user.Provider,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList()
        };

        var ownedInventoriesAll = await _inventoryService.GetOwnedInventoriesAsync(user.Id) ?? new List<Inventory>();

        var totalCount = ownedInventoriesAll.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)ownedPageSize));

        ownedPage = Math.Max(1, ownedPage);
        ownedPage = Math.Min(ownedPage, totalPages);

        var pagedOwned = ownedInventoriesAll
            .Skip((ownedPage - 1) * ownedPageSize)
            .Take(ownedPageSize)
            .ToList();

        ViewBag.OwnedInventories = pagedOwned;
        ViewBag.OwnedTotalPages = totalPages;
        ViewBag.OwnedCurrentPage = ownedPage;
        ViewBag.OwnedTotalCount = totalCount;
        ViewBag.OwnedPageSize = ownedPageSize;

        ViewBag.AccessibleCurrentPage = 1;
        ViewBag.CurrentSort = "";

        return View(vm);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult BlockedUser()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out.", User.FindFirstValue(ClaimTypes.NameIdentifier));
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> LogoutConfirm()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
