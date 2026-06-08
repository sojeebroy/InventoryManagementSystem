namespace Inventory_Management_System.Models;

public class Discussion
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Inventory? Inventory { get; set; }
    public virtual ApplicationUser? User { get; set; }
}

