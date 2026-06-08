using Inventory_Management_System.Models;

namespace Inventory_Management_System.Services.Interfaces
{
    public interface IItemService
    {
        Task<Item?> GetItemByIdAsync(int id);
        Task<List<Item>> GetInventoryItemsAsync(int inventoryId, int page = 1, int pageSize = 20);
        Task<int> GetInventoryItemsCountAsync(int inventoryId);
        Task<Item> CreateItemAsync(Item item);
        Task<Item> UpdateItemAsync(Item item);
        Task DeleteItemAsync(int id);
        Task<List<Item>> SearchItemsAsync(string searchTerm, int inventoryId);
        Task<string> GenerateUniqueCustomIdAsync(int inventoryId, List<CustomIdElement> format);
    }
}
