namespace Inventory_Management_System.Models.DTOs
{
    public class FieldMappingReportDto
    {
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        public List<FieldMappingDto> Fields { get; set; } = new();
        public Dictionary<string, int> ItemsWithDataInField { get; set; } = new();
    }

    public class FieldMappingDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldTitle { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public int ItemsWithData { get; set; }
        public bool IsVisibleInTable { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class FieldRemapDto
    {
        public int FieldId { get; set; }
        public string OldPropertyName { get; set; } = string.Empty;
        public string NewPropertyName { get; set; } = string.Empty;
        public int AffectedItems { get; set; }
    }

    public class FieldCleanupReportDto
    {
        public int FieldId { get; set; }
        public string FieldTitle { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public int ItemsCleared { get; set; }
        public DateTime ClearedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
    }
}

