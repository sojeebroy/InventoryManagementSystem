namespace Inventory_Management_System.Services.Interfaces
{
    public interface IInventoryAuthorizationService
    {
        Task<bool> IsAdminAsync(string userId);
        Task<bool> IsInventoryOwnerAsync(int inventoryId, string userId);
        Task<bool> CanEditInventoryAsync(int inventoryId, string userId);
        Task<bool> CanAddItemAsync(int inventoryId, string userId);
    }
}
