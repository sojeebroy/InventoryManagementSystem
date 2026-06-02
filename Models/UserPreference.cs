namespace Inventory_Management_System.Models;

public class UserPreference
{
    public string UserId { get; set; } = string.Empty;

    public string Theme { get; set; } = "light";

    public bool UseSystemTheme { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser? User { get; set; }

    public bool IsValidTheme() => Theme is "light" or "dark";
}

