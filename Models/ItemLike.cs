namespace Inventory_Management_System.Models;

public class ItemLike
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    public virtual Item? Item { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
