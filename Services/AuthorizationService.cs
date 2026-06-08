using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;

public class InventoryAuthorizationService : IInventoryAuthorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public InventoryAuthorizationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> IsAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsInventoryOwnerAsync(int inventoryId, string userId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.Id == inventoryId);
        return inventory?.OwnerId == userId;
    }

    public async Task<bool> CanEditInventoryAsync(int inventoryId, string userId)
    {
        if (await IsAdminAsync(userId))
            return true;

        if (await IsInventoryOwnerAsync(inventoryId, userId))
            return true;

        var access = await _context.InventoryAccesses
            .FirstOrDefaultAsync(ia => ia.InventoryId == inventoryId && 
                                       ia.UserId == userId && 
                                       ia.AccessLevel >= AccessLevel.Edit);
        return access != null;
    }

    public async Task<bool> CanAddItemAsync(int inventoryId, string userId)
    {
        return await CanEditInventoryAsync(inventoryId, userId);
    }
}
