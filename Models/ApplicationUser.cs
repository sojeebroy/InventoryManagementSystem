using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_System.Models;

public class ApplicationUser : IdentityUser
{
    public string? Provider { get; set; }
    public string? ProviderUserId { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public string? PreferredLanguage { get; set; } = "en";
    public string? Theme { get; set; } = "light";

    public virtual ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
    public virtual ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
    public virtual ICollection<Item> CreatedItems { get; set; } = new List<Item>();
    public virtual ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    public virtual ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}

