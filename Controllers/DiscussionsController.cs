using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;
namespace Inventory_Management_System.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;
    private readonly IInventoryService _inventoryService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DiscussionsController(
        IDiscussionService discussionService,
        IInventoryService inventoryService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _discussionService = discussionService;
        _inventoryService = inventoryService;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateDiscussionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var inventory = await _inventoryService.GetInventoryByIdAsync(dto.InventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        var discussion = new Discussion
        {
            InventoryId = dto.InventoryId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _discussionService.AddDiscussionAsync(discussion);
        return CreatedAtAction(nameof(GetDiscussions), new { inventoryId = dto.InventoryId }, discussion);
    }

    [HttpGet("inventory/{inventoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscussions(int inventoryId)
    {
        var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        // Check access
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (inventory.Visibility == VisibilityType.Private && 
            (string.IsNullOrEmpty(userId) || 
             (inventory.OwnerId != userId && 
              !inventory.AccessControls.Any(ac => ac.UserId == userId))))
        {
            return Forbid();
        }

        var discussions = await _discussionService.GetInventoryDiscussionsAsync(inventoryId);
        return Ok(discussions);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var discussion = _context.Discussions.FirstOrDefault(d => d.Id == id);
        if (discussion == null)
            return NotFound();

        // Only creator or inventory owner can delete
        if (discussion.UserId != userId)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(discussion.InventoryId);
            if (inventory?.OwnerId != userId)
                return Forbid();
        }

        await _discussionService.DeleteDiscussionAsync(id);
        return NoContent();
    }
}
