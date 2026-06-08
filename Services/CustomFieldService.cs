using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;

namespace Inventory_Management_System.Services
{
    public class CustomFieldService : ICustomFieldService
    {
        private readonly ApplicationDbContext _context;
        private const int MaxFieldsPerType = 3;

        public CustomFieldService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomField>> GetInventoryCustomFieldsAsync(int inventoryId)
        {
            return await _context.CustomFields
                .Where(f => f.InventoryId == inventoryId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CustomField?> GetCustomFieldAsync(int id)
        {
            return await _context.CustomFields.FindAsync(id);
        }

        public async Task<CustomField> CreateCustomFieldAsync(CustomField field)
        {
            if (!CanAddFieldOfType(field.InventoryId, field.FieldType))
            {
                throw new InvalidOperationException(
                    $"Cannot add more fields of type {field.FieldType}. Maximum {MaxFieldsPerType} allowed.");
            }

            field.FieldName = GenerateFieldName(field.InventoryId, field.FieldType);

            var maxOrder = await _context.CustomFields
                .Where(f => f.InventoryId == field.InventoryId)
                .MaxAsync(f => (int?)f.DisplayOrder) ?? 0;
            field.DisplayOrder = maxOrder + 1;

            _context.CustomFields.Add(field);
            await _context.SaveChangesAsync();

            return field;
        }

        public async Task<CustomField> UpdateCustomFieldAsync(CustomField field)
        {
            var existing = await _context.CustomFields.FindAsync(field.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Custom field with id {field.Id} not found.");
            }

            existing.Title = field.Title;
            existing.Description = field.Description;
            existing.IsVisibleInTable = field.IsVisibleInTable;
            existing.DisplayOrder = field.DisplayOrder;

            _context.CustomFields.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task DeleteCustomFieldAsync(int id)
        {
            var field = await _context.CustomFields.FindAsync(id);
            if (field != null)
            {
                await ClearFieldDataFromItemsAsync(field);

                _context.CustomFields.Remove(field);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ClearFieldDataFromItemsAsync(CustomField field)
        {
            try
            {
                var items = await _context.Items
                    .Where(i => i.InventoryId == field.InventoryId)
                    .ToListAsync();

                if (!items.Any())
                    return;

                var columnProperty = CustomFieldMappingService.GetItemPropertyForFieldName(field.FieldName);

                foreach (var item in items)
                {
                    var prop = typeof(Item).GetProperty(columnProperty);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(item, null);
                    }
                }

                _context.Items.UpdateRange(items);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[CustomFieldService] Cleared {items.Count} items for deleted field '{field.Title}' (FieldName: {field.FieldName})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomFieldService] Error clearing field data: {ex.Message}");
            }
        }

        private string GetItemPropertyForFieldName(string? fieldName)
        {
            return CustomFieldMappingService.GetItemPropertyForFieldName(fieldName);
        }

        public async Task ReorderCustomFieldsAsync(int inventoryId, List<(int id, int order)> orderedFields)
        {
            var fields = await _context.CustomFields
                .Where(f => f.InventoryId == inventoryId)
                .ToListAsync();

            foreach (var (fieldId, order) in orderedFields)
            {
                var field = fields.FirstOrDefault(f => f.Id == fieldId);
                if (field != null)
                {
                    field.DisplayOrder = order;
                }
            }

            _context.CustomFields.UpdateRange(fields);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderCustomFieldsAsync(int inventoryId, List<FieldReorderDto> orderedFields)
        {
            var fields = await _context.CustomFields
                .Where(f => f.InventoryId == inventoryId)
                .ToListAsync();

            foreach (var reorderDto in orderedFields)
            {
                var field = fields.FirstOrDefault(f => f.Id == reorderDto.Id);
                if (field != null)
                {
                    field.DisplayOrder = reorderDto.Order;
                }
            }

            _context.CustomFields.UpdateRange(fields);
            await _context.SaveChangesAsync();
        }

        public int GetFieldCountByType(int inventoryId, CustomFieldType type)
        {
            return _context.CustomFields
                .Where(f => f.InventoryId == inventoryId && f.FieldType == type)
                .Count();
        }

        public bool CanAddFieldOfType(int inventoryId, CustomFieldType type)
        {
            var count = GetFieldCountByType(inventoryId, type);
            return count < MaxFieldsPerType;
        }

        private string GenerateFieldName(int inventoryId, CustomFieldType type)
        {
            var count = GetFieldCountByType(inventoryId, type) + 1;

            return type switch
            {
                CustomFieldType.SingleLineText => $"custom_string{count}_value",
                CustomFieldType.MultiLineText => $"custom_text{count}_value",
                CustomFieldType.Numeric => $"custom_number{count}_value",
                CustomFieldType.Boolean => $"custom_bool{count}_value",
                CustomFieldType.Link => $"custom_link{count}_value",
                _ => $"custom_field_{count}"
            };
        }
    }
}

