using Inventory_Management_System.Models;

namespace Inventory_Management_System.Services.Interfaces
{
    public interface IDiscussionService
    {
        Task<List<Discussion>> GetInventoryDiscussionsAsync(int inventoryId, int page = 1, int pageSize = 5);
        Task<int> GetInventoryDiscussionsCountAsync(int inventoryId);
        Task<Discussion> AddDiscussionAsync(Discussion discussion);
        Task DeleteDiscussionAsync(int id);
    }
}

