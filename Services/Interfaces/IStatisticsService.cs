namespace Inventory_Management_System.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<InventoryStatistics> GetInventoryStatisticsAsync(int inventoryId);

    }
}
