using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Microsoft.EntityFrameworkCore;
namespace Inventory_Management_System.Services.Interfaces;

public class CustomIdService : ICustomIdService
{
    private readonly Random _random = new();
    private readonly ApplicationDbContext _context;

    public CustomIdService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string GenerateCustomId(int inventoryId, List<CustomIdElement> format)
    {
        var sb = new StringBuilder();

        foreach (var element in format)
        {
            sb.Append(GenerateElement(inventoryId, element));
        }

        return sb.ToString();
    }

    public bool ValidateCustomId(string customId, List<CustomIdElement> format)
    {
        if (string.IsNullOrEmpty(customId))
            return false;

        var minLength = format.Sum(e => e.GetMinLength());
        return customId.Length >= minLength;
    }

    public string GetPreviewCustomId(List<CustomIdElement> format)
    {
        var sb = new StringBuilder();

        foreach (var element in format)
        {
            sb.Append(element.GetPreview());
        }

        return sb.ToString();
    }

    private string GenerateElement(int inventoryId, CustomIdElement element)
    {
        return element.Type switch
        {
            CustomIdElementType.FixedText => element.Value ?? string.Empty,
            CustomIdElementType.RandomNumbers20Bit => GenerateRandomHex(5),
            CustomIdElementType.RandomNumbers32Bit => GenerateRandomHex(8),
            CustomIdElementType.RandomNumbers6Digit => GenerateRandomDecimal(6),
            CustomIdElementType.RandomNumbers9Digit => GenerateRandomDecimal(9),
            CustomIdElementType.Guid => Guid.NewGuid().ToString("N").Substring(0, element.Length ?? 36),
            CustomIdElementType.DateTime => DateTime.UtcNow.ToString(element.Value ?? "yyyyMMdd"),
            CustomIdElementType.SequenceNumber => GenerateSequenceNumber(inventoryId, element.Length ?? 3),
            _ => string.Empty
        };
    }

    private string GenerateRandomHex(int length)
    {
        const string hexChars = "0123456789ABCDEF";
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(hexChars[_random.Next(hexChars.Length)]);
        }
        return sb.ToString();
    }

    private string GenerateRandomDecimal(int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(_random.Next(0, 10));
        }
        return sb.ToString();
    }

    private string GenerateSequenceNumber(int inventoryId, int length)
    {
        var maxSequence = _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .Select(i => i.CustomId)
            .AsEnumerable()
            .Select(id => ExtractSequenceFromId(id))
            .Max() ?? 0;

        var nextSequence = maxSequence + 1;
        return nextSequence.ToString().PadLeft(length, '0');
    }

    private int? ExtractSequenceFromId(string customId)
    {
        if (string.IsNullOrEmpty(customId))
            return null;

        // Try to parse the last digits as sequence
        var digits = new string(customId.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
        return int.TryParse(digits, out var seq) ? seq : null;
    }
}

public class CustomIdElement
{
    [JsonPropertyName("type")]
    public CustomIdElementType Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    [JsonPropertyName("padding")]
    public int? Padding { get; set; }

    public int GetMinLength() => Type switch
    {
        CustomIdElementType.FixedText => Value?.Length ?? 0,
        CustomIdElementType.RandomNumbers20Bit => 5,  // 5 hex digits
        CustomIdElementType.RandomNumbers32Bit => 8,  // 8 hex digits
        CustomIdElementType.RandomNumbers6Digit => 6, // 6 decimal digits
        CustomIdElementType.RandomNumbers9Digit => 9, // 9 decimal digits
        CustomIdElementType.Guid => Length ?? 36,
        CustomIdElementType.DateTime => Value?.Length ?? 8,
        CustomIdElementType.SequenceNumber => Length ?? 3,
        _ => 0
    };

    public string GetPreview() => Type switch
    {
        CustomIdElementType.FixedText => Value ?? "[TEXT]",
        CustomIdElementType.RandomNumbers20Bit => "A7E3A",           // 5-char hex (X5 format)
        CustomIdElementType.RandomNumbers32Bit => "E74FA329",        // 8-char hex (X8 format)
        CustomIdElementType.RandomNumbers6Digit => "013245",         // 6-digit decimal (D6)
        CustomIdElementType.RandomNumbers9Digit => "001234567",      // 9-digit decimal (D9)
        CustomIdElementType.Guid => Guid.NewGuid().ToString().Substring(0, Math.Min(Length ?? 36, 36)),
        CustomIdElementType.DateTime => DateTime.UtcNow.ToString(Value ?? "yyyyMMdd"),
        CustomIdElementType.SequenceNumber => "001",                 // Default D3 format
        _ => "[?]"
    };
}

public enum CustomIdElementType
{
    FixedText,
    RandomNumbers20Bit,
    RandomNumbers32Bit,
    RandomNumbers6Digit,
    RandomNumbers9Digit,
    Guid,
    DateTime,
    SequenceNumber
}
