using System;
using System.Collections.Generic;

namespace Inventory_Management_System.Models.ViewModels;

public class UserProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Provider { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}