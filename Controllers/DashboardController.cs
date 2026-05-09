using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;
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

    public async Task<IActionResult> Index(string sortBy = "date", string filterBy = "all", int ownedPage = 1, int accessiblePage = 1)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        const int pageSize = 10;

        var ownedInventories = await _inventoryService.GetOwnedInventoriesAsync(userId);
        var accessibleInventories = await _inventoryService.GetAccessibleInventoriesAsync(userId);

        // Remove owned inventories from accessible list
        var ownedIds = ownedInventories.Select(i => i.Id).ToHashSet();
        var otherAccessible = accessibleInventories.Where(i => !ownedIds.Contains(i.Id)).ToList();

        // Apply sorting
        ownedInventories = ApplySorting(ownedInventories, sortBy);
        otherAccessible = ApplySorting(otherAccessible, sortBy);

        // Calculate pagination for owned inventories
        var ownedTotalCount = ownedInventories.Count;
        var ownedTotalPages = (int)Math.Ceiling(ownedTotalCount / (double)pageSize);
        var ownedPaginatedList = ownedInventories
            .Skip((ownedPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Calculate pagination for accessible inventories
        var accessibleTotalCount = otherAccessible.Count;
        var accessibleTotalPages = (int)Math.Ceiling(accessibleTotalCount / (double)pageSize);
        var accessiblePaginatedList = otherAccessible
            .Skip((accessiblePage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.OwnedInventories = ownedPaginatedList;
        ViewBag.OwnedCurrentPage = ownedPage;
        ViewBag.OwnedTotalPages = ownedTotalPages;
        ViewBag.OwnedTotalCount = ownedTotalCount;
        ViewBag.OwnedPageSize = pageSize;

        ViewBag.AccessibleInventories = accessiblePaginatedList;
        ViewBag.AccessibleCurrentPage = accessiblePage;
        ViewBag.AccessibleTotalPages = accessibleTotalPages;
        ViewBag.AccessibleTotalCount = accessibleTotalCount;
        ViewBag.AccessiblePageSize = pageSize;

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
