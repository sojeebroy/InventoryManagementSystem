namespace Inventory_Management_System.Models;

public class Inventory
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public VisibilityType Visibility { get; set; } = VisibilityType.Private;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    public string CustomIdFormat { get; set; } = string.Empty;

    public virtual ApplicationUser? Owner { get; set; }
    public virtual ICollection<InventoryTag> Tags { get; set; } = new List<InventoryTag>();
    public virtual ICollection<InventoryAccess> AccessControls { get; set; } = new List<InventoryAccess>();
    public virtual ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    public virtual ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
}

public enum VisibilityType
{
    Public,
    Private
}

