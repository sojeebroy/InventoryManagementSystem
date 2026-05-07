namespace Inventory_Management_System.Models.ViewModels;

public class UserAdminViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Provider { get; set; }
    public List<string> Roles { get; set; } = new();
}
