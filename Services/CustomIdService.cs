using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventory_Management_System.Services;

public interface ICustomIdService
{
    string GenerateCustomId(int inventoryId, List<CustomIdElement> format);
    bool ValidateCustomId(string customId, List<CustomIdElement> format);
    string GetPreviewCustomId(List<CustomIdElement> format);
}

public class CustomIdService : ICustomIdService
{
    private readonly Random _random = new();

    public string GenerateCustomId(int inventoryId, List<CustomIdElement> format)
    {
        var sb = new StringBuilder();

        foreach (var element in format)
        {
            sb.Append(GenerateElement(element));
        }

        return sb.ToString();
    }

    public bool ValidateCustomId(string customId, List<CustomIdElement> format)
    {
        if (string.IsNullOrEmpty(customId))
            return false;

        var generated = GetPreviewCustomId(format);
        // Basic validation - structure should match pattern
        return customId.Length >= format.Sum(e => e.GetMinLength());
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

    private string GenerateElement(CustomIdElement element)
    {
        return element.Type switch
        {
            CustomIdElementType.FixedText => element.Value ?? string.Empty,
            CustomIdElementType.RandomNumbers20Bit => GenerateRandomNumbers(5),
            CustomIdElementType.RandomNumbers32Bit => GenerateRandomNumbers(10),
            CustomIdElementType.RandomNumbers6Digit => GenerateRandomNumbers(6),
            CustomIdElementType.RandomNumbers9Digit => GenerateRandomNumbers(9),
            CustomIdElementType.Guid => Guid.NewGuid().ToString("N").Substring(0, element.Length ?? 8),
            CustomIdElementType.DateTime => DateTime.UtcNow.ToString(element.Value ?? "yyyyMMdd"),
            CustomIdElementType.SequenceNumber => GenerateSequenceNumber(element.Length ?? 5),
            _ => string.Empty
        };
    }

    private string GenerateRandomNumbers(int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(_random.Next(0, 10));
        }
        return sb.ToString();
    }

    private string GenerateSequenceNumber(int length)
    {
        var number = _random.Next(1, (int)Math.Pow(10, length));
        return number.ToString().PadLeft(length, '0');
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
        CustomIdElementType.RandomNumbers20Bit => 5,
        CustomIdElementType.RandomNumbers32Bit => 10,
        CustomIdElementType.RandomNumbers6Digit => 6,
        CustomIdElementType.RandomNumbers9Digit => 9,
        CustomIdElementType.Guid => Length ?? 8,
        CustomIdElementType.DateTime => Value?.Length ?? 8,
        CustomIdElementType.SequenceNumber => Length ?? 5,
        _ => 0
    };

    public string GetPreview() => Type switch
    {
        CustomIdElementType.FixedText => Value ?? "[TEXT]",
        CustomIdElementType.RandomNumbers20Bit => "XXXXX",
        CustomIdElementType.RandomNumbers32Bit => "XXXXXXXXXX",
        CustomIdElementType.RandomNumbers6Digit => "XXXXXX",
        CustomIdElementType.RandomNumbers9Digit => "XXXXXXXXX",
        CustomIdElementType.Guid => new string('X', Length ?? 8),
        CustomIdElementType.DateTime => DateTime.UtcNow.ToString(Value ?? "yyyyMMdd"),
        CustomIdElementType.SequenceNumber => new string('0', Length ?? 5),
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
