namespace Inventory_Management_System.Models.ViewModels;

public class HomePageViewModel
{
    public List<InventoryCardDto> LatestInventories { get; set; } = new();
    public List<InventoryCardDto> TopPopularInventories { get; set; } = new();
    public List<TagCloudDto> TagCloud { get; set; } = new();
}

public class InventoryCardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class TagCloudDto
{
    public string TagName { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string Size { get; set; } = "Small"; 
}

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<InventoryCardDto> Results { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; }
    public int TotalResults { get; set; }
}

public class TagFilterViewModel
{
    public string Tag { get; set; } = string.Empty;
    public List<InventoryCardDto> Results { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; }
}