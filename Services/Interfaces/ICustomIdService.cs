namespace Inventory_Management_System.Services.Interfaces
{
    public interface ICustomIdService
    {
        string GenerateCustomId(int inventoryId, List<CustomIdElement> format);
        bool ValidateCustomId(string customId, List<CustomIdElement> format);
        string GetPreviewCustomId(List<CustomIdElement> format);
    }
}
