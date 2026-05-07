using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;
using System.Text.Json;

namespace Inventory_Management_System.Controllers;

[Authorize]
public class InventoriesController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryAuthorizationService _authService;
    private readonly IItemService _itemService;
    private readonly IDiscussionService _discussionService;
    private readonly IStatisticsService _statisticsService;
    private readonly UserManager<ApplicationUser> _userManager;

    public InventoriesController(
        IInventoryService inventoryService,
        IInventoryAuthorizationService authService,
        IItemService itemService,
        IDiscussionService discussionService,
        IStatisticsService statisticsService,
        UserManager<ApplicationUser> userManager)
    {
        _inventoryService = inventoryService;
        _authService = authService;
        _itemService = itemService;
        _discussionService = discussionService;
        _statisticsService = statisticsService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string searchTerm = "")
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        List<Inventory> inventories;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            inventories = await _inventoryService.SearchInventoriesAsync(searchTerm);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            inventories = await _inventoryService.GetAccessibleInventoriesAsync(userId);
        }
        else
        {
            inventories = new List<Inventory>();
        }

        ViewBag.SearchTerm = searchTerm;
        return View(inventories);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        // Check access
        if (inventory.Visibility == VisibilityType.Private &&
            (string.IsNullOrEmpty(userId) ||
             (inventory.OwnerId != userId &&
              !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        ViewBag.CanEdit = !string.IsNullOrEmpty(userId) && 
            await _authService.CanEditInventoryAsync(id, userId);
        ViewBag.CanAddItems = ViewBag.CanEdit;
        ViewBag.UserId = userId;

        return View(inventory);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var inventory = new Inventory
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            Visibility = Enum.Parse<VisibilityType>(dto.VisibilityType),
            OwnerId = userId,
            CustomIdFormat = JsonSerializer.Serialize(new List<CustomIdElement>())
        };

        var created = await _inventoryService.CreateInventoryAsync(inventory);

        // Add tags if provided - create tags directly in database
        if (dto.Tags.Any())
        {
            foreach (var tagName in dto.Tags)
            {                
                if (!string.IsNullOrWhiteSpace(tagName))
                {
                    var inventoryTag = new InventoryTag
                    {
                        InventoryId = created.Id,
                        Tag = tagName.Trim()
                    };
                    // Add directly to context and save
                    await _inventoryService.AddTagAsync(inventoryTag);
                }           
            }
        }

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(id, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        if (inventory == null)
            return NotFound();

        var dto = new UpdateInventoryDto
        {
            Id = inventory.Id,
            Title = inventory.Title,
            Description = inventory.Description,
            Category = inventory.Category,
            ImageUrl = inventory.ImageUrl,
            VisibilityType = inventory.Visibility.ToString(),
            TagsInput = string.Join(", ", inventory.Tags.Select(t => t.Tag)),
            Version = inventory.Version
        };

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateInventoryDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(dto.Id, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdForUpdateAsync(dto.Id);
        if (inventory == null)
            return NotFound();

        inventory.Title = dto.Title;
        inventory.Description = dto.Description;
        inventory.Category = dto.Category;
        inventory.ImageUrl = dto.ImageUrl;
        inventory.Visibility = Enum.Parse<VisibilityType>(dto.VisibilityType);
        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Version = dto.Version;

        try
        {
            await _inventoryService.UpdateInventoryAsync(inventory);

            // Update tags - delete old ones and add new ones
            if (dto.Tags != null && dto.Tags.Any())
            {
                await _inventoryService.DeleteInventoryTagsAsync(dto.Id);

                foreach (var tagName in dto.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        var inventoryTag = new InventoryTag
                        {
                            InventoryId = dto.Id,
                            Tag = tagName.Trim()
                        };
                        await _inventoryService.AddTagAsync(inventoryTag);
                    }
                }
            }
            else
            {
                // If no tags provided, delete all existing tags
                await _inventoryService.DeleteInventoryTagsAsync(dto.Id);
            }

            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.IsInventoryOwnerAsync(id, userId))
            return Forbid();

        await _inventoryService.DeleteInventoryAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Items(int id, int page = 1)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        if (inventory.Visibility == VisibilityType.Private && 
            (string.IsNullOrEmpty(userId) || 
             (inventory.OwnerId != userId && 
              !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        var items = await _itemService.GetInventoryItemsAsync(id, page);
        var totalItems = await _itemService.GetInventoryItemsCountAsync(id);

        ViewBag.Inventory = inventory;
        ViewBag.CanEdit = !string.IsNullOrEmpty(userId) && 
            await _authService.CanEditInventoryAsync(id, userId);
        ViewBag.UserId = userId;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / 20.0);
        ViewBag.CurrentPage = page;

        return View(items);
    }

    public async Task<IActionResult> Discussion(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        if (inventory.Visibility == VisibilityType.Private && 
            (string.IsNullOrEmpty(userId) || 
             (inventory.OwnerId != userId && 
              !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        var discussions = await _discussionService.GetInventoryDiscussionsAsync(id);
        ViewBag.Inventory = inventory;
        ViewBag.CanAdd = !string.IsNullOrEmpty(userId);
        ViewBag.UserId = userId;

        return View(discussions);
    }

    public async Task<IActionResult> Settings(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(id, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        if (inventory == null)
            return NotFound();

        return View(inventory);
    }

    public async Task<IActionResult> Statistics(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        if (inventory.Visibility == VisibilityType.Private && 
            (string.IsNullOrEmpty(userId) || 
             (inventory.OwnerId != userId && 
              !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        var stats = await _statisticsService.GetInventoryStatisticsAsync(id);
        ViewBag.Inventory = inventory;

        return View(stats);
    }
}
