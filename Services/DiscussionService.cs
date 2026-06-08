using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
namespace Inventory_Management_System.Services.Interfaces;

public class DiscussionService : IDiscussionService
{
    private readonly ApplicationDbContext _context;

    public DiscussionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Discussion>> GetInventoryDiscussionsAsync(int inventoryId, int page = 1, int pageSize = 5)
    {
        return await _context.Discussions
            .AsNoTracking()
            .Where(d => d.InventoryId == inventoryId)
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Discussion>> GetInventoryDiscussionsSinceAsync(int inventoryId, DateTime since)
    {
        return await _context.Discussions
            .AsNoTracking()
            .Where(d => d.InventoryId == inventoryId && d.CreatedAt > since)
            .Include(d => d.User)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetInventoryDiscussionsCountAsync(int inventoryId)
    {
        return await _context.Discussions
            .Where(d => d.InventoryId == inventoryId)
            .CountAsync();
    }

    public async Task<Discussion> AddDiscussionAsync(Discussion discussion)
    {
        discussion.CreatedAt = DateTime.UtcNow;
        _context.Discussions.Add(discussion);
        await _context.SaveChangesAsync();

        await _context.Entry(discussion).Reference(d => d.User).LoadAsync();
        return discussion;
    }

    public async Task DeleteDiscussionAsync(int id)
    {
        var discussion = await _context.Discussions.FindAsync(id);
        if (discussion != null)
        {
            _context.Discussions.Remove(discussion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Discussion?> GetDiscussionByIdAsync(int id)
    {
        return await _context.Discussions
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}

