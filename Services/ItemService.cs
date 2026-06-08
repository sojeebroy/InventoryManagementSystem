using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
namespace Inventory_Management_System.Services.Interfaces;

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ICustomIdService _customIdService;

    public ItemService(ApplicationDbContext context, ICustomIdService customIdService)
    {
        _context = context;
        _customIdService = customIdService;
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _context.Items
            .AsNoTracking()
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Item>> GetInventoryItemsAsync(int inventoryId, int page = 1, int pageSize = 5)
    {
        return await _context.Items
            .AsNoTracking()
            .Where(i => i.InventoryId == inventoryId)
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.CustomFields)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetInventoryItemsCountAsync(int inventoryId)
    {
        return await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .CountAsync();
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateItemAsync(Item item)
    {
        _context.Items.Update(item);
        try
        {
            await _context.SaveChangesAsync();
            return item;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Item was modified by another user. Please refresh and try again.");
        }
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Item>> SearchItemsAsync(string searchTerm, int inventoryId)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Item>();

        var term = searchTerm.ToLower();
        return await _context.Items
            .AsNoTracking()
            .Where(i => i.InventoryId == inventoryId &&
                       ((i.CustomId ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomString1Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomString2Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomString3Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomText1Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomText2Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomText3Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomLink1Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomLink2Value ?? string.Empty).ToLower().Contains(term) ||
                        (i.CustomLink3Value ?? string.Empty).ToLower().Contains(term)))
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<string> GenerateUniqueCustomIdAsync(int inventoryId, List<CustomIdElement> format)
    {
        if (format == null || format.Count == 0)
            return string.Empty;

        string customId;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            customId = _customIdService.GenerateCustomId(inventoryId, format);
            var exists = await _context.Items
                .AnyAsync(i => i.InventoryId == inventoryId && i.CustomId == customId);

            if (!exists)
                return customId;

            attempts++;
        } while (attempts < maxAttempts);

        throw new InvalidOperationException("Unable to generate unique custom ID after maximum attempts.");
    }
}

