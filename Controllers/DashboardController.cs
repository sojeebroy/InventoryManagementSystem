using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services;

namespace Inventory_Management_System.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IInventoryService inventoryService, UserManager<ApplicationUser> userManager)
    {
        _inventoryService = inventoryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string sortBy = "date", string filterBy = "all")
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var ownedInventories = await _inventoryService.GetOwnedInventoriesAsync(userId);
        var accessibleInventories = await _inventoryService.GetAccessibleInventoriesAsync(userId);

        // Remove owned inventories from accessible list
        var ownedIds = ownedInventories.Select(i => i.Id).ToHashSet();
        var otherAccessible = accessibleInventories.Where(i => !ownedIds.Contains(i.Id)).ToList();

        // Apply sorting
        ownedInventories = ApplySorting(ownedInventories, sortBy);
        otherAccessible = ApplySorting(otherAccessible, sortBy);

        ViewBag.OwnedInventories = ownedInventories;
        ViewBag.AccessibleInventories = otherAccessible;
        ViewBag.CurrentSort = sortBy;
        ViewBag.CurrentFilter = filterBy;

        return View();
    }

    private List<Inventory> ApplySorting(List<Inventory> inventories, string sortBy)
    {
        return sortBy switch
        {
            "title" => inventories.OrderBy(i => i.Title).ToList(),
            "newest" => inventories.OrderByDescending(i => i.CreatedAt).ToList(),
            "oldest" => inventories.OrderBy(i => i.CreatedAt).ToList(),
            _ => inventories.OrderByDescending(i => i.UpdatedAt).ToList()
        };
    }
}
