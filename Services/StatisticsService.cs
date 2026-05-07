using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
namespace Inventory_Management_System.Services.Interfaces;


public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;

    public StatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryStatistics> GetInventoryStatisticsAsync(int inventoryId)
    {
        var stats = new InventoryStatistics { InventoryId = inventoryId };

        // Total items
        stats.TotalItems = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .CountAsync();

        // Get all items for custom field analysis
        var items = await _context.Items
            .AsNoTracking()
            .Where(i => i.InventoryId == inventoryId)
            .ToListAsync();

        // Numeric field statistics
        CalculateNumericStatistics(stats, items);

        // String field statistics
        CalculateStringStatistics(stats, items);

        // Get custom fields for context
        stats.CustomFields = await _context.CustomFields
            .AsNoTracking()
            .Where(cf => cf.InventoryId == inventoryId)
            .ToListAsync();

        return stats;
    }

    private void CalculateNumericStatistics(InventoryStatistics stats, List<Item> items)
    {
        // Numeric field 1
        var numeric1Values = items
            .Where(i => i.CustomNumber1Value.HasValue)
            .Select(i => i.CustomNumber1Value.Value)
            .ToList();

        if (numeric1Values.Any())
        {
            stats.Number1Stats = new NumericFieldStats
            {
                Average = Math.Round(numeric1Values.Average(), 2),
                Min = numeric1Values.Min(),
                Max = numeric1Values.Max(),
                Count = numeric1Values.Count
            };
        }

        // Numeric field 2
        var numeric2Values = items
            .Where(i => i.CustomNumber2Value.HasValue)
            .Select(i => i.CustomNumber2Value.Value)
            .ToList();

        if (numeric2Values.Any())
        {
            stats.Number2Stats = new NumericFieldStats
            {
                Average = Math.Round(numeric2Values.Average(), 2),
                Min = numeric2Values.Min(),
                Max = numeric2Values.Max(),
                Count = numeric2Values.Count
            };
        }

        // Numeric field 3
        var numeric3Values = items
            .Where(i => i.CustomNumber3Value.HasValue)
            .Select(i => i.CustomNumber3Value.Value)
            .ToList();

        if (numeric3Values.Any())
        {
            stats.Number3Stats = new NumericFieldStats
            {
                Average = Math.Round(numeric3Values.Average(), 2),
                Min = numeric3Values.Min(),
                Max = numeric3Values.Max(),
                Count = numeric3Values.Count
            };
        }
    }

    private void CalculateStringStatistics(InventoryStatistics stats, List<Item> items)
    {
        // String field 1
        var string1Values = items
            .Where(i => !string.IsNullOrEmpty(i.CustomString1Value))
            .GroupBy(i => i.CustomString1Value)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new StringFieldFrequency { Value = g.Key!, Count = g.Count() })
            .ToList();

        if (string1Values.Any())
            stats.String1Stats = string1Values;

        // String field 2
        var string2Values = items
            .Where(i => !string.IsNullOrEmpty(i.CustomString2Value))
            .GroupBy(i => i.CustomString2Value)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new StringFieldFrequency { Value = g.Key!, Count = g.Count() })
            .ToList();

        if (string2Values.Any())
            stats.String2Stats = string2Values;

        // String field 3
        var string3Values = items
            .Where(i => !string.IsNullOrEmpty(i.CustomString3Value))
            .GroupBy(i => i.CustomString3Value)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new StringFieldFrequency { Value = g.Key!, Count = g.Count() })
            .ToList();

        if (string3Values.Any())
            stats.String3Stats = string3Values;
    }
}

public class InventoryStatistics
{
    public int InventoryId { get; set; }
    public int TotalItems { get; set; }

    public NumericFieldStats? Number1Stats { get; set; }
    public NumericFieldStats? Number2Stats { get; set; }
    public NumericFieldStats? Number3Stats { get; set; }

    public List<StringFieldFrequency> String1Stats { get; set; } = new();
    public List<StringFieldFrequency> String2Stats { get; set; } = new();
    public List<StringFieldFrequency> String3Stats { get; set; } = new();

    public List<CustomField> CustomFields { get; set; } = new();
}

public class NumericFieldStats
{
    public decimal Average { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public int Count { get; set; }
}

public class StringFieldFrequency
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
}
