namespace Inventory_Management_System.Models;

public class InventoryAccess
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AccessLevel AccessLevel { get; set; } = AccessLevel.View;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public virtual Inventory? Inventory { get; set; }
    public virtual ApplicationUser? User { get; set; }
}

public enum AccessLevel
{
    View,
    Edit,
    Admin
}
