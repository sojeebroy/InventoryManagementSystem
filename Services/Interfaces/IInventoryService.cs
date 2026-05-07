using Inventory_Management_System.Models;

namespace Inventory_Management_System.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory?> GetInventoryByIdForUpdateAsync(int id);
        Task<List<Inventory>> GetAccessibleInventoriesAsync(string userId);
        Task<List<Inventory>> GetOwnedInventoriesAsync(string userId);
        Task<Inventory> CreateInventoryAsync(Inventory inventory);
        Task<Inventory> UpdateInventoryAsync(Inventory inventory);
        Task DeleteInventoryAsync(int id);
        Task<bool> CanAccessInventoryAsync(int inventoryId, string userId, AccessLevel requiredLevel = AccessLevel.View);
        Task<List<Inventory>> SearchInventoriesAsync(string searchTerm);
        Task AddTagAsync(InventoryTag tag);
        Task DeleteInventoryTagsAsync(int inventoryId);
    }
}
