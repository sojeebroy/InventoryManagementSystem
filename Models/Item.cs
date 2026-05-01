namespace Inventory_Management_System.Models;

public class Item
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string CustomId { get; set; } = string.Empty; // Unique per inventory
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1; // Optimistic locking

    // Custom field values - fixed column approach
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

    // Navigation properties
    public virtual Inventory? Inventory { get; set; }
    public virtual ApplicationUser? CreatedBy { get; set; }
    public virtual ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
}
