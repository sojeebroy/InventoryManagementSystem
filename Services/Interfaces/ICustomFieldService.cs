using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;

namespace Inventory_Management_System.Services.Interfaces
{
    public interface ICustomFieldService
    {
        Task<List<CustomField>> GetInventoryCustomFieldsAsync(int inventoryId);
        Task<CustomField?> GetCustomFieldAsync(int id);
        Task<CustomField> CreateCustomFieldAsync(CustomField field);
        Task<CustomField> UpdateCustomFieldAsync(CustomField field);
        Task DeleteCustomFieldAsync(int id);
        Task ReorderCustomFieldsAsync(int inventoryId, List<(int id, int order)> orderedFields);
        Task ReorderCustomFieldsAsync(int inventoryId, List<FieldReorderDto> orderedFields);
        int GetFieldCountByType(int inventoryId, CustomFieldType type);
        bool CanAddFieldOfType(int inventoryId, CustomFieldType type);
    }
}
