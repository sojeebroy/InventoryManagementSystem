using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_System.Models;

public class ApplicationUser : IdentityUser
{
    // OAuth Provider Information
    public string? Provider { get; set; } // "Google" or "Facebook"
    public string? ProviderUserId { get; set; } // Provider's unique ID
    public string? ProfilePictureUrl { get; set; }

    // User Profile
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Account Status
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // User Preferences
    public string? PreferredLanguage { get; set; } = "en";
    public string? Theme { get; set; } = "light";

    // Navigation properties
    public virtual ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
    public virtual ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
    public virtual ICollection<Item> CreatedItems { get; set; } = new List<Item>();
    public virtual ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    public virtual ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
