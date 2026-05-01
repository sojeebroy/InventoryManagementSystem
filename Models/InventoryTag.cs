namespace Inventory_Management_System.Models;

public class InventoryTag
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string Tag { get; set; } = string.Empty;

    public virtual Inventory? Inventory { get; set; }
}
