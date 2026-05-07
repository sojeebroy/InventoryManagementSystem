using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;
using System.Text.Json;

namespace Inventory_Management_System.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryAuthorizationService _authService;
    private readonly ICustomIdService _customIdService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemsController(
        IItemService itemService,
        IInventoryService inventoryService,
        IInventoryAuthorizationService authService,
        ICustomIdService customIdService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _itemService = itemService;
        _inventoryService = inventoryService;
        _authService = authService;
        _customIdService = customIdService;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!await _authService.CanAddItemAsync(dto.InventoryId, userId))
            return Forbid();

        var inventory = await _inventoryService.GetInventoryByIdAsync(dto.InventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        // Generate custom ID
        List<CustomIdElement> format = new();
        if (!string.IsNullOrEmpty(inventory.CustomIdFormat))
        {
            format = JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdFormat) ?? new();
        }

        var customId = await _itemService.GenerateUniqueCustomIdAsync(dto.InventoryId, format);

        var item = new Item
        {
            InventoryId = dto.InventoryId,
            CustomId = customId,
            CreatedById = userId,
            CustomString1Value = dto.CustomString1Value,
            CustomString2Value = dto.CustomString2Value,
            CustomString3Value = dto.CustomString3Value,
            CustomText1Value = dto.CustomText1Value,
            CustomText2Value = dto.CustomText2Value,
            CustomText3Value = dto.CustomText3Value,
            CustomNumber1Value = dto.CustomNumber1Value,
            CustomNumber2Value = dto.CustomNumber2Value,
            CustomNumber3Value = dto.CustomNumber3Value,
            CustomBool1Value = dto.CustomBool1Value,
            CustomBool2Value = dto.CustomBool2Value,
            CustomBool3Value = dto.CustomBool3Value,
            CustomLink1Value = dto.CustomLink1Value,
            CustomLink2Value = dto.CustomLink2Value,
            CustomLink3Value = dto.CustomLink3Value
        };

        await _itemService.CreateItemAsync(item);
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        // Check access to inventory
        var canAccess = await _inventoryService.CanAccessInventoryAsync(item.InventoryId, 
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");

        if (!canAccess)
            return Forbid();

        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        if (!await _authService.CanAddItemAsync(item.InventoryId, userId))
            return Forbid();

        item.CustomString1Value = dto.CustomString1Value;
        item.CustomString2Value = dto.CustomString2Value;
        item.CustomString3Value = dto.CustomString3Value;
        item.CustomText1Value = dto.CustomText1Value;
        item.CustomText2Value = dto.CustomText2Value;
        item.CustomText3Value = dto.CustomText3Value;
        item.CustomNumber1Value = dto.CustomNumber1Value;
        item.CustomNumber2Value = dto.CustomNumber2Value;
        item.CustomNumber3Value = dto.CustomNumber3Value;
        item.CustomBool1Value = dto.CustomBool1Value;
        item.CustomBool2Value = dto.CustomBool2Value;
        item.CustomBool3Value = dto.CustomBool3Value;
        item.CustomLink1Value = dto.CustomLink1Value;
        item.CustomLink2Value = dto.CustomLink2Value;
        item.CustomLink3Value = dto.CustomLink3Value;
        item.UpdatedAt = DateTime.UtcNow;
        item.Version = dto.Version;

        try
        {
            await _itemService.UpdateItemAsync(item);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        if (!await _authService.CanAddItemAsync(item.InventoryId, userId))
            return Forbid();

        await _itemService.DeleteItemAsync(id);
        return NoContent();
    }

    [HttpPost("{itemId}/like")]
    public async Task<IActionResult> LikeItem(int itemId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var item = await _itemService.GetItemByIdAsync(itemId);
        if (item == null)
            return NotFound();

        // Check if already liked
        var existingLike = _context.ItemLikes
            .FirstOrDefault(il => il.ItemId == itemId && il.UserId == userId);

        if (existingLike != null)
        {
            _context.ItemLikes.Remove(existingLike);
        }
        else
        {
            var like = new ItemLike
            {
                ItemId = itemId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };
            _context.ItemLikes.Add(like);
        }

        await _context.SaveChangesAsync();

        var likeCount = _context.ItemLikes.Count(il => il.ItemId == itemId);
        return Ok(new { liked = existingLike == null, likeCount });
    }

    [HttpGet("{itemId}/likes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLikes(int itemId)
    {
        var likeCount = _context.ItemLikes
            .Where(il => il.ItemId == itemId)
            .Count();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isLiked = !string.IsNullOrEmpty(userId) && _context.ItemLikes
            .Any(il => il.ItemId == itemId && il.UserId == userId);

        return Ok(new { count = likeCount, isLiked });
    }
}
