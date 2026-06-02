namespace Inventory_Management_System.Models;

public class CustomField
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CustomFieldType FieldType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisibleInTable { get; set; } = true;
    public string? FieldName { get; set; }

    public virtual Inventory? Inventory { get; set; }
}

public enum CustomFieldType
{
    SingleLineText,
    MultiLineText,
    Numeric,
    Boolean,
    Link
}

