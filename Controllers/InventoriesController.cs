using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;
using Inventory_Management_System.Services;
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
    private readonly ICustomFieldService _customFieldService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICloudinaryService _cloudinaryService;

    public InventoriesController(
        IInventoryService inventoryService,
        IInventoryAuthorizationService authService,
        IItemService itemService,
        IDiscussionService discussionService,
        IStatisticsService statisticsService,
        ICustomFieldService customFieldService,
        UserManager<ApplicationUser> userManager,
        ICloudinaryService cloudinaryService)
    {
        _inventoryService = inventoryService;
        _authService = authService;
        _itemService = itemService;
        _discussionService = discussionService;
        _statisticsService = statisticsService;
        _customFieldService = customFieldService;
        _userManager = userManager;
        _cloudinaryService = cloudinaryService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        List<Inventory> inventories;
        if (!string.IsNullOrEmpty(searchTerm))
            inventories = await _inventoryService.SearchInventoriesAsync(searchTerm);
        else if (!string.IsNullOrEmpty(userId))
            inventories = await _inventoryService.GetAccessibleInventoriesAsync(userId);
        else
            inventories = new List<Inventory>();

        const int pageSize = 10;
        var totalCount = inventories.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var paginatedList = inventories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.PageSize = pageSize;

        return View(paginatedList);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
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

        if (dto.ImageFile == null || dto.ImageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Image file is required to create an inventory.");
            return View(dto);
        }

        string? imageUrl = null;

        try
        {
            imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("ImageFile", ex.Message);
            return View(dto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("ImageFile", ex.Message);
            return View(dto);
        }

        var inventory = new Inventory
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            ImageUrl = imageUrl,
            Visibility = Enum.Parse<VisibilityType>(dto.VisibilityType),
            OwnerId = userId,
            CustomIdFormat = JsonSerializer.Serialize(new List<CustomIdElement>())
        };

        var created = await _inventoryService.CreateInventoryAsync(inventory);

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

        string? imageUrl = dto.ImageUrl;

        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            try
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("ImageFile", ex.Message);
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("ImageFile", ex.Message);
                return View(dto);
            }
        }

        inventory.Title = dto.Title;
        inventory.Description = dto.Description;
        inventory.Category = dto.Category;
        inventory.ImageUrl = imageUrl;
        inventory.Visibility = Enum.Parse<VisibilityType>(dto.VisibilityType);
        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Version = dto.Version;

        try
        {
            await _inventoryService.UpdateInventoryAsync(inventory);

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

    public async Task<IActionResult> Items(int id, int page = 1, string search = "")
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

        List<Item> items;
        int totalItems;

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var searchResults = await _itemService.SearchItemsAsync(search, id);
            totalItems = searchResults.Count;
            items = searchResults
                .Skip((page - 1) * 5)
                .Take(5)
                .ToList();
        }
        else
        {
            items = await _itemService.GetInventoryItemsAsync(id, page, pageSize: 5);
            totalItems = await _itemService.GetInventoryItemsCountAsync(id);
        }

        ViewBag.Inventory = inventory;
        ViewBag.CanEdit = !string.IsNullOrEmpty(userId) &&
            await _authService.CanEditInventoryAsync(id, userId);
        ViewBag.UserId = userId;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / 5.0);
        ViewBag.CurrentPage = page;
        ViewBag.TotalItems = totalItems;
        ViewBag.SearchTerm = search;

        return View("Items", items);
    }

    public async Task<IActionResult> ItemsPartial(int id, int page = 1)
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

        var items = await _itemService.GetInventoryItemsAsync(id, page, pageSize: 5);
        var totalItems = await _itemService.GetInventoryItemsCountAsync(id);

        ViewBag.Inventory = inventory;
        ViewBag.CanEdit = !string.IsNullOrEmpty(userId) &&
            await _authService.CanEditInventoryAsync(id, userId);
        ViewBag.UserId = userId;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / 5.0);
        ViewBag.CurrentPage = page;
        ViewBag.TotalItems = totalItems;

        return PartialView("_Items", items);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Discussion(int id, int page = 1)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;
        var isAdmin = User.IsInRole("Admin");
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        bool isOwner = inventory.OwnerId == userId;
        bool hasAccess = inventory.AccessControls?.Any(ac => ac.UserId == userId) == true;
        bool isPublic = inventory.Visibility == VisibilityType.Public;

        if (!isPublic && !isOwner && !isAdmin && !hasAccess)
            return Forbid();

        bool canAdd = currentUser != null;

        const int pageSize = 5;
        page = Math.Max(1, page);
        var totalDiscussions = await _discussionService.GetInventoryDiscussionsCountAsync(id);
        var totalPages = (int)Math.Ceiling(totalDiscussions / (double)pageSize);
        totalPages = Math.Max(1, totalPages);
        page = Math.Min(page, totalPages);

        var discussions = await _discussionService.GetInventoryDiscussionsAsync(id, page, pageSize);

        ViewBag.Inventory = inventory;
        ViewBag.CanAdd = canAdd;
        ViewBag.UserId = userId ?? "";
        ViewBag.IsAdmin = isAdmin;
        ViewBag.IsOwner = isOwner;
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.TotalDiscussions = totalDiscussions;

        return View("Discussion", discussions);
    }

    [AllowAnonymous]
    public async Task<IActionResult> DiscussionPartial(int id, int page = 1)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;
        var isAdmin = User.IsInRole("Admin");
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);

        if (inventory == null)
            return NotFound();

        bool isOwner = inventory.OwnerId == userId;
        bool hasAccess = inventory.AccessControls?.Any(ac => ac.UserId == userId) == true;
        bool isPublic = inventory.Visibility == VisibilityType.Public;

        if (!isPublic && !isOwner && !isAdmin && !hasAccess)
            return Forbid();

        bool canAdd = currentUser != null;

        const int pageSize = 5;
        page = Math.Max(1, page);
        var totalDiscussions = await _discussionService.GetInventoryDiscussionsCountAsync(id);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalDiscussions / (double)pageSize));
        page = Math.Min(page, totalPages);

        var discussions = await _discussionService.GetInventoryDiscussionsAsync(id, page, pageSize);

        ViewBag.Inventory = inventory;
        ViewBag.CanAdd = canAdd;
        ViewBag.UserId = userId ?? "";
        ViewBag.IsAdmin = isAdmin;
        ViewBag.IsOwner = isOwner;
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.TotalDiscussions = totalDiscussions;

        return PartialView("_Discussion", discussions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostDiscussion(int inventoryId, string content)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;

        if (currentUser == null)
        {
            TempData["DiscussionError"] = "You must be logged in to post.";
            return RedirectToAction("Discussion", new { id = inventoryId });
        }

        var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
        if (inventory == null)
            return NotFound();

if (string.IsNullOrWhiteSpace(content))
        {
            TempData["DiscussionError"] = "Comment cannot be empty.";
            return RedirectToAction("Discussion", new { id = inventoryId });
        }

        if (content.Trim().Length > 5000)
        {
            TempData["DiscussionError"] = "Comment must be 5000 characters or fewer.";
            return RedirectToAction("Discussion", new { id = inventoryId });
        }

        var discussion = new Discussion
        {
            InventoryId = inventoryId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _discussionService.AddDiscussionAsync(discussion);
        TempData["DiscussionSuccess"] = "Your comment was posted successfully!";

        return RedirectToAction("Discussion", new { id = inventoryId, page = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDiscussion(int id, int inventoryId, int returnPage = 1)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;
        var isAdmin = User.IsInRole("Admin");

        if (currentUser == null)
            return RedirectToAction("Login", "Account");

        var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
        if (inventory == null)
            return NotFound();

        var allOnPage = await _discussionService.GetInventoryDiscussionsAsync(inventoryId, 1, int.MaxValue);
        var discussion = allOnPage.FirstOrDefault(d => d.Id == id);

        if (discussion == null)
            return NotFound();

        if (discussion.UserId != userId && !isAdmin)
        {
            TempData["DiscussionError"] = "You don't have permission to delete this post.";
            return RedirectToAction("Discussion", new { id = inventoryId, page = returnPage });
        }

        await _discussionService.DeleteDiscussionAsync(id);
        TempData["DiscussionSuccess"] = "Comment deleted.";

        const int pageSize = 5;
        var total = await _discussionService.GetInventoryDiscussionsCountAsync(inventoryId);
        var maxPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var safePage = Math.Min(returnPage, maxPage);

        return RedirectToAction("Discussion", new { id = inventoryId, page = safePage });
    }

    public async Task<IActionResult> Settings(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(id, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        if (inventory == null)
            return NotFound();

        return View("Settings", inventory);
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
        var totalDiscussions = await _discussionService.GetInventoryDiscussionsCountAsync(id);

        ViewBag.Inventory = inventory;
        ViewBag.TotalDiscussions = totalDiscussions;

        return View("Statistics", stats);
    }

    public async Task<IActionResult> StatisticsPartial(int id)
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
        var totalDiscussions = await _discussionService.GetInventoryDiscussionsCountAsync(id);

        ViewBag.Inventory = inventory;
        ViewBag.TotalDiscussions = totalDiscussions;

        return PartialView("_Statistics", stats);
    }

    [HttpPost]
    [Route("/Inventories/SaveCustomIdFormat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomIdFormat([FromQuery] int id, [FromBody] JsonElement format)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(id, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdForUpdateAsync(id);
        if (inventory == null)
            return NotFound();

        if (format.ValueKind == JsonValueKind.Null || format.ValueKind == JsonValueKind.Undefined)
        {
            return BadRequest(new { success = false, error = "Format cannot be empty" });
        }

        if (format.ValueKind != JsonValueKind.Array)
        {
            return BadRequest(new { success = false, error = "Format must be an array" });
        }

        using var doc = JsonDocument.Parse(format.GetRawText());
        var array = doc.RootElement;

        if (array.GetArrayLength() == 0)
        {
            return BadRequest(new { success = false, error = "Format cannot be empty" });
        }

        List<CustomIdElement> formatList = new();

        try
        {
            foreach (var item in array.EnumerateArray())
            {
                if (!item.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
                {
                    return BadRequest(new { success = false, error = "Each element must have a 'type' property (string)" });
                }

                string typeStr = typeElement.GetString() ?? "";

                if (!Enum.TryParse<CustomIdElementType>(typeStr, ignoreCase: true, out var typeEnum))
                {
                    return BadRequest(new { success = false, error = $"Invalid type: {typeStr}" });
                }

                var element = new CustomIdElement { Type = typeEnum };

                if (item.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                {
                    element.Text = textElement.GetString();
                    if (string.IsNullOrEmpty(element.Value))
                        element.Value = element.Text;
                }

                if (item.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.String)
                {
                    element.Value = valueElement.GetString();
                    if (string.IsNullOrEmpty(element.Text))
                        element.Text = element.Value;
                }

                if (item.TryGetProperty("length", out var lengthElement) && lengthElement.ValueKind == JsonValueKind.Number)
                {
                    element.Length = lengthElement.GetInt32();
                }

                if (item.TryGetProperty("start", out var startElement) && startElement.ValueKind == JsonValueKind.Number)
                {
                    element.Start = startElement.GetInt32();
                }

                if (item.TryGetProperty("pad", out var padElement) && padElement.ValueKind == JsonValueKind.Number)
                {
                    element.Padding = padElement.GetInt32();
                }

                if (item.TryGetProperty("padding", out var paddingElement) && paddingElement.ValueKind == JsonValueKind.Number)
                {
                    element.Padding = element.Padding ?? paddingElement.GetInt32();
                }

                if (item.TryGetProperty("property", out var propElement) && propElement.ValueKind == JsonValueKind.String)
                {
                    element.Property = propElement.GetString() ?? "";
                }

                if (item.TryGetProperty("prefix", out var prefixElement) && prefixElement.ValueKind == JsonValueKind.String)
                {
                    element.Prefix = prefixElement.GetString() ?? "";
                }

                formatList.Add(element);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing format: {ex.Message}");
            return BadRequest(new { success = false, error = $"Invalid format data: {ex.Message}" });
        }

        inventory.CustomIdFormat = JsonSerializer.Serialize(formatList);
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventoryService.UpdateInventoryAsync(inventory);

        var preview = formatList.Aggregate(new System.Text.StringBuilder(), (sb, el) => sb.Append(el.GetPreview())).ToString();

        return Json(new { success = true, preview = preview });
    }

    [HttpGet]
    public async Task<IActionResult> CheckCustomIdFormat(int id)
    {
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        if (inventory == null)
            return NotFound(new { hasCustomIdFormat = false });

        bool hasFormat = !string.IsNullOrEmpty(inventory.CustomIdFormat);
        return Ok(new { hasCustomIdFormat = hasFormat });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetCustomFields(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(id, userId))
            return Forbid();

        var fields = await _customFieldService.GetInventoryCustomFieldsAsync(id);
        return Json(fields);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateCustomField(int inventoryId, [FromBody] JsonElement payload)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(inventoryId, userId))
            return Forbid();

        if (payload.ValueKind == JsonValueKind.Null || payload.ValueKind == JsonValueKind.Undefined)
            return Json(new { success = false, error = "Field payload is required" });

        try
        {
            string title = payload.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? "" : "";
            string? description = payload.TryGetProperty("description", out var descEl) ? descEl.GetString() : null;
            string fieldTypeStr = payload.TryGetProperty("fieldType", out var typeEl) ? typeEl.GetString() ?? "SingleLineText" : "SingleLineText";
            bool isVisibleInTable = payload.TryGetProperty("isVisibleInTable", out var visEl) ? visEl.GetBoolean() : true;

            if (string.IsNullOrWhiteSpace(title))
                return Json(new { success = false, error = "Title is required" });

            if (!Enum.TryParse<CustomFieldType>(fieldTypeStr, ignoreCase: true, out var fieldType))
                return Json(new { success = false, error = $"Invalid fieldType: {fieldTypeStr}" });

            var field = new CustomField
            {
                InventoryId = inventoryId,
                Title = title,
                Description = description,
                FieldType = fieldType,
                IsVisibleInTable = isVisibleInTable,
                DisplayOrder = 0
            };

            var created = await _customFieldService.CreateCustomFieldAsync(field);
            return Json(new
            {
                success = true,
                field = new
                {
                    id = created.Id,
                    title = created.Title,
                    description = created.Description,
                    fieldType = created.FieldType.ToString(),
                    isVisibleInTable = created.IsVisibleInTable,
                    displayOrder = created.DisplayOrder
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateCustomField] Error: {ex}");
            return Json(new { success = false, error = $"Error: {ex.Message}" });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateCustomField(int id, [FromBody] JsonElement payload)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var existing = await _customFieldService.GetCustomFieldAsync(id);
        if (existing == null)
            return NotFound();

        if (!await _authService.CanEditInventoryAsync(existing.InventoryId, userId))
            return Forbid();

        existing.Title = payload.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? existing.Title : existing.Title;
        existing.Description = payload.TryGetProperty("description", out var descEl) ? descEl.GetString() : existing.Description;
        existing.IsVisibleInTable = payload.TryGetProperty("isVisibleInTable", out var visEl) ? visEl.GetBoolean() : existing.IsVisibleInTable;

        var updated = await _customFieldService.UpdateCustomFieldAsync(existing);
        return Json(new
        {
            success = true,
            field = new
            {
                id = updated.Id,
                title = updated.Title,
                description = updated.Description,
                fieldType = updated.FieldType.ToString(),
                isVisibleInTable = updated.IsVisibleInTable,
                displayOrder = updated.DisplayOrder
            }
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteCustomField(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var field = await _customFieldService.GetCustomFieldAsync(id);
        if (field == null)
            return NotFound();

        if (!await _authService.CanEditInventoryAsync(field.InventoryId, userId))
            return Forbid();

        await _customFieldService.DeleteCustomFieldAsync(id);
        return Json(new { success = true });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReorderCustomFields(int inventoryId, [FromBody] List<FieldReorderDto> order)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !await _authService.CanEditInventoryAsync(inventoryId, userId))
            return Forbid();

        await _customFieldService.ReorderCustomFieldsAsync(inventoryId, order);
        return Json(new { success = true });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GetDiscussionsSince(int inventoryId, [FromBody] DateTime since)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);

        if (inventory == null)
            return NotFound();

        bool isPublic = inventory.Visibility == VisibilityType.Public;
        if (!isPublic && (string.IsNullOrEmpty(userId) ||
            (inventory.OwnerId != userId &&
             !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        var newDiscussions = await _discussionService.GetInventoryDiscussionsSinceAsync(inventoryId, since);
        return Json(newDiscussions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetVisibility(int id, VisibilityType visibility)
    {
        var inventory = await _inventoryService.GetInventoryByIdForUpdateAsync(id);
        if (inventory == null)
            return NotFound();

        if (inventory.Visibility == visibility)
            return Json(new { success = true, message = "No change" });

        inventory.Visibility = visibility;
        inventory.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _inventoryService.UpdateInventoryAsync(inventory);
            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false, message = "Error saving visibility" });
        }
    }
}

