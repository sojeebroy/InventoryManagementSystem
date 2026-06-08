using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Services.Interfaces;

namespace Inventory_Management_System.Services
{
    public class FieldMappingReportService
    {
        private readonly ApplicationDbContext _context;

        public FieldMappingReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FieldMappingReportDto> GetInventoryFieldMappingReportAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new InvalidOperationException($"Inventory {inventoryId} not found");

            var fields = await _context.CustomFields
                .Where(f => f.InventoryId == inventoryId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            var report = new FieldMappingReportDto
            {
                InventoryId = inventoryId,
                InventoryTitle = inventory.Title,
                Fields = new()
            };

            foreach (var field in fields)
            {
                var propertyName = CustomFieldMappingService.GetItemPropertyForFieldName(field.FieldName);
                var itemsWithData = await CountItemsWithDataInFieldAsync(inventoryId, propertyName);

                report.Fields.Add(new FieldMappingDto
                {
                    FieldId = field.Id,
                    FieldName = field.FieldName ?? "",
                    FieldTitle = field.Title,
                    FieldType = field.FieldType.ToString(),
                    PropertyName = propertyName,
                    ItemsWithData = itemsWithData,
                    IsVisibleInTable = field.IsVisibleInTable,
                    DisplayOrder = field.DisplayOrder
                });

                if (itemsWithData > 0)
                {
                    report.ItemsWithDataInField[propertyName] = itemsWithData;
                }
            }

            return report;
        }

        public async Task<List<FieldMappingReportDto>> GetAllInventoriesFieldMappingReportAsync()
        {
            var inventories = await _context.Inventories.ToListAsync();
            var reports = new List<FieldMappingReportDto>();

            foreach (var inventory in inventories)
            {
                try
                {
                    var report = await GetInventoryFieldMappingReportAsync(inventory.Id);
                    if (report.Fields.Any())
                    {
                        reports.Add(report);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FieldMappingReportService] Error generating report for inventory {inventory.Id}: {ex.Message}");
                }
            }

            return reports;
        }

        private async Task<int> CountItemsWithDataInFieldAsync(int inventoryId, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return 0;

            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

            var prop = typeof(Item).GetProperty(propertyName);
            if (prop == null)
                return 0;

            return items.Count(item =>
            {
                var value = prop.GetValue(item);
                if (value == null)
                    return false;

                if (value is string str)
                    return !string.IsNullOrWhiteSpace(str);

                if (value is decimal dec)
                    return dec != 0;

                if (value is double dbl)
                    return dbl != 0;

                return true;
            });
        }

        public async Task<Dictionary<string, PropertyUsageDto>> GetPropertyUsageReportAsync()
        {
            var allProperties = CustomFieldMappingService.GetCustomItemProperties();
            var report = new Dictionary<string, PropertyUsageDto>();

            var allItems = await _context.Items.ToListAsync();

            foreach (var prop in allProperties)
            {
                var itemProperty = typeof(Item).GetProperty(prop.PropertyName);
                if (itemProperty == null)
                    continue;

                var usedCount = allItems.Count(item =>
                {
                    var value = itemProperty.GetValue(item);
                    if (value == null)
                        return false;

                    if (value is string str)
                        return !string.IsNullOrWhiteSpace(str);

                    return true;
                });

                report[prop.PropertyName] = new PropertyUsageDto
                {
                    PropertyName = prop.PropertyName,
                    FieldType = prop.FieldType.ToString(),
                    TotalItems = allItems.Count,
                    UsedItems = usedCount,
                    PercentageUsed = allItems.Count > 0 ? (usedCount * 100.0m) / allItems.Count : 0
                };
            }

            return report;
        }

        public async Task<FieldRemapDto> SimulateFieldRemapAsync(int inventoryId, string sourceProperty, string targetProperty)
        {
            var sourceProp = typeof(Item).GetProperty(sourceProperty);
            var targetProp = typeof(Item).GetProperty(targetProperty);

            if (sourceProp == null || targetProp == null)
                throw new InvalidOperationException("Invalid property names");

            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

            var affectedCount = items.Count(item =>
            {
                var value = sourceProp.GetValue(item);
                if (value == null)
                    return false;

                if (value is string str)
                    return !string.IsNullOrWhiteSpace(str);

                return true;
            });

            return new FieldRemapDto
            {
                OldPropertyName = sourceProperty,
                NewPropertyName = targetProperty,
                AffectedItems = affectedCount
            };
        }

        public async Task<FieldRemapDto> ExecuteFieldRemapAsync(int inventoryId, string sourceProperty, string targetProperty)
        {
            var sourceProp = typeof(Item).GetProperty(sourceProperty);
            var targetProp = typeof(Item).GetProperty(targetProperty);

            if (sourceProp == null || targetProp == null)
                throw new InvalidOperationException("Invalid property names");

            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

            int affectedCount = 0;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var item in items)
                    {
                        var sourceValue = sourceProp.GetValue(item);
                        var targetValue = targetProp.GetValue(item);

                        if (sourceValue != null && targetValue == null)
                        {
                            targetProp.SetValue(item, sourceValue);
                            sourceProp.SetValue(item, null);
                            affectedCount++;
                        }
                    }

                    _context.Items.UpdateRange(items);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"[FieldMappingReportService] Field remapping completed: {affectedCount} items migrated");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[FieldMappingReportService] Field remapping failed: {ex.Message}");
                    throw;
                }
            }

            return new FieldRemapDto
            {
                OldPropertyName = sourceProperty,
                NewPropertyName = targetProperty,
                AffectedItems = affectedCount
            };
        }
    }

    public class PropertyUsageDto
    {
        public string PropertyName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int UsedItems { get; set; }
        public decimal PercentageUsed { get; set; }
    }
}

