namespace Inventory_Management_System.Models.DTOs;

public class CreateInventoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string VisibilityType { get; set; } = "Private";
    public List<string> Tags { get; set; } = new();
}

public class UpdateInventoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string VisibilityType { get; set; } = "Private";
    public List<string> Tags { get; set; } = new();
    public int Version { get; set; }
}

public class InventoryAccessDto
{
    public int InventoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = "View";
}

public class CreateItemDto
{
    public int InventoryId { get; set; }
    public string? CustomString1Value { get; set; }
    public string? CustomString2Value { get; set; }
    public string? CustomString3Value { get; set; }
    public string? CustomText1Value { get; set; }
    public string? CustomText2Value { get; set; }
    public string? CustomText3Value { get; set; }
    public decimal? CustomNumber1Value { get; set; }
    public decimal? CustomNumber2Value { get; set; }
    public decimal? CustomNumber3Value { get; set; }
    public bool? CustomBool1Value { get; set; }
    public bool? CustomBool2Value { get; set; }
    public bool? CustomBool3Value { get; set; }
    public string? CustomLink1Value { get; set; }
    public string? CustomLink2Value { get; set; }
    public string? CustomLink3Value { get; set; }
}

public class UpdateItemDto
{
    public int Id { get; set; }
    public string? CustomString1Value { get; set; }
    public string? CustomString2Value { get; set; }
    public string? CustomString3Value { get; set; }
    public string? CustomText1Value { get; set; }
    public string? CustomText2Value { get; set; }
    public string? CustomText3Value { get; set; }
    public decimal? CustomNumber1Value { get; set; }
    public decimal? CustomNumber2Value { get; set; }
    public decimal? CustomNumber3Value { get; set; }
    public bool? CustomBool1Value { get; set; }
    public bool? CustomBool2Value { get; set; }
    public bool? CustomBool3Value { get; set; }
    public string? CustomLink1Value { get; set; }
    public string? CustomLink2Value { get; set; }
    public string? CustomLink3Value { get; set; }
    public int Version { get; set; }
}

public class CreateDiscussionDto
{
    public int InventoryId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ItemLikeDto
{
    public int ItemId { get; set; }
}
