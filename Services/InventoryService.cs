using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
namespace Inventory_Management_System.Services.Interfaces;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetInventoryByIdAsync(int id)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Owner)
            .Include(i => i.Tags)
            .Include(i => i.AccessControls)
            .Include(i => i.CustomFields.OrderBy(cf => cf.DisplayOrder))
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Inventory?> GetInventoryByIdForUpdateAsync(int id)
    {
        return await _context.Inventories
            .Include(i => i.Owner)
            .Include(i => i.Tags)
            .Include(i => i.AccessControls)
            .Include(i => i.CustomFields.OrderBy(cf => cf.DisplayOrder))
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Inventory>> GetAccessibleInventoriesAsync(string userId)
    {
        var query = _context.Inventories
            .AsNoTracking()
            .Where(i => i.Visibility == VisibilityType.Public || 
                       i.OwnerId == userId ||
                       i.AccessControls.Any(ac => ac.UserId == userId))
            .Include(i => i.Owner)
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt);

        return await query.ToListAsync();
    }

    public async Task<List<Inventory>> GetOwnedInventoriesAsync(string userId)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Where(i => i.OwnerId == userId)
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        try
        {
            await _context.SaveChangesAsync();
            return inventory;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Inventory was modified by another user. Please refresh and try again.");
        }
    }

    public async Task DeleteInventoryAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CanAccessInventoryAsync(int inventoryId, string userId, AccessLevel requiredLevel = AccessLevel.View)
    {
        var inventory = await _context.Inventories
            .Include(i => i.AccessControls)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
            return false;

        if (inventory.OwnerId == userId)
            return true;

        if (inventory.Visibility == VisibilityType.Public && requiredLevel == AccessLevel.View)
            return true;

        var access = inventory.AccessControls.FirstOrDefault(ac => ac.UserId == userId);
        if (access == null)
            return false;

        return access.AccessLevel >= requiredLevel;
    }

    public async Task<List<Inventory>> SearchInventoriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Inventory>();

        var term = searchTerm.ToLower();
        return await _context.Inventories
            .AsNoTracking()
            .Where(i => i.Visibility == VisibilityType.Public &&
                       (i.Title.ToLower().Contains(term) ||
                        i.Description.ToLower().Contains(term) ||
                        i.Tags.Any(t => t.Tag.ToLower().Contains(term))))
            .Include(i => i.Owner)
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task AddTagAsync(InventoryTag tag)
    {
        _context.InventoryTags.Add(tag);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInventoryTagsAsync(int inventoryId)
    {
        var tags = await _context.InventoryTags
            .Where(t => t.InventoryId == inventoryId)
            .ToListAsync();

        if (tags.Any())
        {
            _context.InventoryTags.RemoveRange(tags);
            await _context.SaveChangesAsync();
        }
    }
}

