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

    public async Task<List<Discussion>> GetInventoryDiscussionsAsync(int inventoryId)
    {
        return await _context.Discussions
            .AsNoTracking()
            .Where(d => d.InventoryId == inventoryId)
            .Include(d => d.User)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Discussion> AddDiscussionAsync(Discussion discussion)
    {
        _context.Discussions.Add(discussion);
        await _context.SaveChangesAsync();
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
}
