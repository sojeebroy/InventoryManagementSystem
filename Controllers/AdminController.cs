using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.ViewModels;
using System.Security.Claims;

namespace Inventory_Management_System.Controllers;

/// <summary>
/// Admin panel for managing users and system settings
/// Only accessible to users with Admin role
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Admin dashboard - list all users
    /// </summary>
    [HttpGet("")]
    [HttpGet("Users")]
    public async Task<IActionResult> Users(string? searchTerm = null, int page = 1, int pageSize = 20)
    {
        var query = _userManager.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                u.Email.Contains(searchTerm) || 
                u.UserName.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        // Calculate pagination
        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Get page of users
        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Enrich with role information
        var userModels = new List<UserAdminViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userModels.Add(new UserAdminViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsAdmin = roles.Contains("Admin"),
                IsBlocked = user.IsBlocked,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            });
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;

        return View(userModels);
    }

    /// <summary>
    /// View user details
    /// </summary>
    [HttpGet("Users/{id}")]
    public async Task<IActionResult> UserDetail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserAdminViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsAdmin = roles.Contains("Admin"),
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Provider = user.Provider,
            Roles = roles.ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Block a user (prevent login)
    /// </summary>
    [HttpPost("Users/{id}/Block")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Prevent blocking self
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Id == currentUserId)
        {
            return BadRequest("You cannot block yourself");
        }

        user.IsBlocked = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to block user {UserId}", id);
            return BadRequest("Failed to block user");
        }

        _logger.LogInformation("Admin blocked user {UserId} ({Email})", id, user.Email);
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    /// <summary>
    /// Unblock a user
    /// </summary>
    [HttpPost("Users/{id}/Unblock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.IsBlocked = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to unblock user {UserId}", id);
            return BadRequest("Failed to unblock user");
        }

        _logger.LogInformation("Admin unblocked user {UserId} ({Email})", id, user.Email);
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    /// <summary>
    /// Grant admin role to a user
    /// </summary>
    [HttpPost("Users/{id}/GrantAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            return BadRequest("User is already an admin");
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to grant admin role to user {UserId}", id);
            return BadRequest("Failed to grant admin role");
        }

        _logger.LogWarning("Admin granted admin role to user {UserId} ({Email})", id, user.Email);
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    /// <summary>
    /// Revoke admin role from a user
    /// </summary>
    [HttpPost("Users/{id}/RevokeAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin"))
        {
            return BadRequest("User is not an admin");
        }

        // Allow admin to revoke their own role
        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to revoke admin role from user {UserId}", id);
            return BadRequest("Failed to revoke admin role");
        }

        _logger.LogWarning("Admin revoked admin role from user {UserId} ({Email})", id, user.Email);
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    /// <summary>
    /// Delete a user and all associated data
    /// </summary>
    [HttpPost("Users/{id}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Prevent deleting self
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Id == currentUserId)
        {
            return BadRequest("You cannot delete yourself");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to delete user {UserId}", id);
            return BadRequest("Failed to delete user");
        }

        _logger.LogWarning("Admin deleted user {UserId} ({Email})", id, user.Email);
        return RedirectToAction(nameof(Users));
    }
}