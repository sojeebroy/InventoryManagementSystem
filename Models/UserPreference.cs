namespace Inventory_Management_System.Models;

/// <summary>
/// Represents user preferences for theme, language, and UI settings.
/// This is a separate entity to keep user preferences organized and easily extensible.
/// </summary>
public class UserPreference
{
    /// <summary>
    /// Primary key - matches ApplicationUser.Id
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's preferred theme: "light" or "dark"
    /// Default: "light"
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// User's preferred language: "en" (English) or "es" (Spanish)
    /// Default: "en"
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Whether the user prefers system theme detection (follows OS/browser settings)
    /// Default: false
    /// </summary>
    public bool UseSystemTheme { get; set; } = false;

    /// <summary>
    /// Timestamp when preferences were last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to ApplicationUser
    /// </summary>
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Validates that theme is either "light" or "dark"
    /// </summary>
    public bool IsValidTheme() => Theme is "light" or "dark";

    /// <summary>
    /// Validates that language is either "en" or "es"
    /// </summary>
    public bool IsValidLanguage() => Language is "en" or "es";
}
